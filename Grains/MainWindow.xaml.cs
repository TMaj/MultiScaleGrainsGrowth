using Grains.Library.Enums;
using Grains.Library.Processors;
using Grains.Library.Extensions;
using Grains.Utilities.TextHandler;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Grains.Utilities.ImageHandler;

namespace Grains
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Rectangle[,] array;
        private Processor processor;
        private Color[] colorsArray;
        private bool[,] renderingArray;
        private int xDimension;
        private int yDimension;

        public MainWindow()
        {
            this.xDimension = 300;
            this.yDimension = 300;

            this.array = new Rectangle[xDimension, yDimension];
            this.processor = new Processor(xDimension, yDimension);
            this.colorsArray = new Color[10];
            this.renderingArray = new bool[xDimension, yDimension];
            InitializeComponent();
            InitializeRectanglesOnCanvas(xDimension, yDimension);
        }

        private void InitializeRectanglesOnCanvas(int width, int height)
        {
            double rectangeWidth = this.canvas.Width / width;
            double rectangeHeight = this.canvas.Height / height;

            for (int i=0; i< width; i++)
            {
                for (int j=0; j < height; j++)
                {
                    var rect = new Rectangle();
                    rect.Width = rectangeWidth;
                    rect.Height = rectangeHeight;

                    rect.Fill = new SolidColorBrush(Colors.Azure);

                    this.array[i, j] = rect;

                    Canvas.SetLeft(rect, i*rectangeWidth);
                    Canvas.SetTop(rect, j*rectangeHeight);

                    if (!CheckAccess())
                    {
                        Dispatcher.Invoke(() => this.canvas.Children.Add(rect));
                        return;
                    }

                    this.canvas.Children.Add(rect);
                }
            }
        }

        private void InitializeColorsArray(int value)
        {
            this.colorsArray = new Color[value+1];
            var rand = new Random();
            this.colorsArray[0] = Colors.White;

            for (int i = 1; i < value + 1; i++)
            {
                var color = new Color();
                color.R = (byte)rand.Next(0, 255);
                color.G = (byte)rand.Next(0, 255);
                color.B = (byte)rand.Next(0, 255);
                color.A = 255;
                this.colorsArray[i] = color;
            }
        }

        private void RefreshFullArray()
        {
            for (int i = 0; i < xDimension; i++)
            {
                for (int j = 0; j < yDimension; j++)
                {
                    if (this.processor.Array[i, j] !=0 && !this.renderingArray[i, j])
                    {
                        this.array[i, j].Fill = new SolidColorBrush(colorsArray[this.processor.Array[i, j]]);
                        this.renderingArray[i, j] = true;
                    }                    
                }
            }
        }
        
        private void ClearArray()
        {
            for (int i = 0; i < xDimension; i++)
            {
                for (int j = 0; j < yDimension; j++)
                {
                    this.array[i, j].Fill = new SolidColorBrush(colorsArray[0]);
                }
            }
        }

        private void RefreshArray()
        {
            foreach (var cell in processor.UpdatedCells)
            {
                this.array[cell.X, cell.Y].Fill = new SolidColorBrush(colorsArray[this.processor.Array[cell.X, cell.Y]]);
            }
        }

        private async void stepButton_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                this.processor.MakeStep();

            }).ContinueWith((result) => Dispatcher.BeginInvoke((Action)(() => {
                RefreshFullArray();
            })));
          
        }

        private async void randomButton_Click(object sender, RoutedEventArgs e)
        {
            InitializeColorsArray(Convert.ToInt32(this.randomTextBox.Text));
            
            await this.processor.AddRandomGrains(Convert.ToInt32(this.randomTextBox.Text));

            RefreshFullArray();
        }

        private void radioButton2_Checked(object sender, RoutedEventArgs e)
        {
            this.processor.SetNeighbourhood(Neighbourhood.Moore);
        }

        private void radioButton2_Unchecked(object sender, RoutedEventArgs e)
        {
            this.processor.SetNeighbourhood(Neighbourhood.VonNeumann);
        }

        private async void clearButton_Click(object sender, RoutedEventArgs e)
        {
            await this.processor.Clear();

            await Task.Run(() =>
            {                
                this.renderingArray = new bool[xDimension, yDimension];

            }).ContinueWith((result) => Dispatcher.BeginInvoke((Action)(() => {
                this.ClearArray();
            })));
            
        }

        private async void importButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            string path = string.Empty;

            if (openFileDialog.ShowDialog() == true)
            {
                path = openFileDialog.FileName;
                if (File.Exists(path))
                {
                    TextHandler.ImportFromTextFile(this.processor.Array, this.xDimension, this.yDimension, path);
                }
            }

            await Task.Run(() =>
            {
                this.renderingArray = new bool[xDimension, yDimension];
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.ClearArray();
                    InitializeColorsArray(this.processor.Array.Max()); 
                    RefreshFullArray();
                }));
            });
        }

        private void exportButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            string path = string.Empty;

            if (saveFileDialog.ShowDialog() == true)
            {
                path = saveFileDialog.FileName;

                if (!File.Exists(path))
                {
                    File.Create(path).Close();
                }

                TextHandler.ExportToTextFile(this.processor.Array, this.xDimension, this.yDimension, path);
            }            
        }

        private void exportImageButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Png files (*.png)|*.png|All files (*.*)|*.*";
            string path = string.Empty;

            if (saveFileDialog.ShowDialog() == true)
            {
                path = saveFileDialog.FileName;

                if (!File.Exists(path))
                {
                    File.Create(path).Close();
                }

                ImageHandler.ExportToImage(this.canvas, path);
            }
        }

        private async void importImageButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Png files (*.png)|*.png|All files (*.*)|*.*";
            string path = string.Empty;

            if (openFileDialog.ShowDialog() == true)
            {
                path = openFileDialog.FileName;
                if (File.Exists(path))
                {
                    ImageHandler.ImportFromImage(this.processor.Array, this.xDimension, this.yDimension, path);
                }
            }

            await Task.Run(() =>
            {
                this.renderingArray = new bool[xDimension, yDimension];
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.ClearArray();
                    InitializeColorsArray(this.processor.Array.Max());
                    RefreshFullArray();
                }));
            });
        }
    }
}
