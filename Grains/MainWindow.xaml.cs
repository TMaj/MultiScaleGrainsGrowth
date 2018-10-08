using Grains.Library.Enums;
using Grains.Library.Processors;
using System;
using System.Collections.Generic;
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

        public MainWindow()
        {
            this.array = new Rectangle[300, 300];
            this.processor = new Processor(300, 300);
            this.colorsArray = new Color[10];
            InitializeComponent();
            InitializeRectanglesOnCanvas(300, 300);
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
            this.colorsArray[0] = Colors.Aqua;

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
            for (int i = 0; i < 300; i++)
            {
                for (int j = 0; j < 300; j++)
                {
                    this.array[i, j].Fill = new SolidColorBrush(colorsArray[this.processor.Array[i, j]]);
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

        private void stepButton_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                this.processor.MakeStep();

            }).ContinueWith((result) => Dispatcher.BeginInvoke((Action)(() => {
                RefreshArray();
            })));
          
        }

        private void randomButton_Click(object sender, RoutedEventArgs e)
        {
            InitializeColorsArray(Convert.ToInt32(this.randomTextBox.Text));

            this.processor.AddRandomGrains(Convert.ToInt32(this.randomTextBox.Text));

            RefreshArray();
        }

        private void radioButton2_Checked(object sender, RoutedEventArgs e)
        {
            this.processor.SetNeighbourhood(Neighbourhood.Moore);
        }

        private void radioButton2_Unchecked(object sender, RoutedEventArgs e)
        {
            this.processor.SetNeighbourhood(Neighbourhood.VonNeumann);
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                this.processor.Clear();

            }).ContinueWith((result) => Dispatcher.BeginInvoke((Action)(() => {
                this.RefreshFullArray();
            })));
            
        }
    }
}
