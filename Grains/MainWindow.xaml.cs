﻿using Grains.Library.Enums;
using Grains.Library.Extensions;
using Grains.Library.Processors;
using Grains.Utilities.ImageHandler;
using Grains.Utilities.IOHandlers;
using Grains.Utilities.TextHandler;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

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

        private readonly BackgroundWorker backgroundWorker;
        private readonly BackgroundWorker clearingBackgroundWorker;
        private readonly DispatcherTimer dispatcherTimer;

        public MainWindow()
        {
            xDimension = 300;
            yDimension = 300;

            array = new Rectangle[xDimension, yDimension];
            processor = new Processor(xDimension, yDimension);
            colorsArray = new Color[10];
            renderingArray = new bool[xDimension, yDimension];
            backgroundWorker = new BackgroundWorker();
            clearingBackgroundWorker = new BackgroundWorker();

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);

            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += worker_RunWorkerCompleted;

            clearingBackgroundWorker.DoWork += clearingbackgroundWorker_DoWork;

            InitializeComponent();
            InitializeRectanglesOnCanvas(xDimension, yDimension);
            InitializeComboBoxes();
            InitializeColorsArray(0);
        }

        private void InitializeRectanglesOnCanvas(int width, int height)
        {
            double rectangeWidth = canvas.Width / width;
            double rectangeHeight = canvas.Height / height;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var rect = new Rectangle();
                    rect.Width = rectangeWidth;
                    rect.Height = rectangeHeight;

                    rect.Fill = new SolidColorBrush(Colors.Azure);

                    array[i, j] = rect;

                    Canvas.SetLeft(rect, i * rectangeWidth);
                    Canvas.SetTop(rect, j * rectangeHeight);

                    if (!CheckAccess())
                    {
                        Dispatcher.Invoke(() => canvas.Children.Add(rect));
                        return;
                    }

                    canvas.Children.Add(rect);
                }
            }
        }

        private void InitializeColorsArray(int value)
        {
            colorsArray = new Color[value + 2];
            var rand = new Random();
            colorsArray[0] = Colors.White;
            colorsArray[1] = Colors.Black;

            for (int i = 2; i < value + 2; i++)
            {
                var color = new Color();
                color.R = (byte)rand.Next(0, 255);
                color.G = (byte)rand.Next(0, 255);
                color.B = (byte)rand.Next(0, 255);
                color.A = 255;
                colorsArray[i] = color;
            }
        }

        private void InitializeComboBoxes()
        {
            var inclusions = new List<Inclusions>
            {
                Inclusions.Circular,
                Inclusions.Square
            };

            inclusionsComboBox.ItemsSource = inclusions;
            inclusionsComboBox.SelectedIndex = 0;

            var substructures = new List<Substructures>
            {
               Substructures.Substructure,
               Substructures.DualPhase
            };

            substructuresComboBox.ItemsSource = substructures;
            substructuresComboBox.SelectedIndex = 0;
        }

        private void RefreshFullArray()
        {

            //Parallel.For(0, xDimension, (i) => {
            //    Parallel.For(0, yDimension, (j) => {
            //        Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            //        {
            //            array[i, j].Fill = new SolidColorBrush(colorsArray[processor.Array[i, j]]);
            //        }));
            //    });
            //});

            for (int i = 0; i < xDimension; i++)
            {
                for (int j = 0; j < yDimension; j++)
                {
                    if (processor.Array[i, j] == 0)
                    {
                        continue;
                    }

                    if (renderingArray[i, j] == true)
                    {
                        continue;
                    }

                    renderingArray[i, j] = true;
                    // if (!renderingArray[i, j])
                    // {
                    array[i, j].Fill = new SolidColorBrush(colorsArray[processor.Array[i, j]]);
                  //      renderingArray[i, j] = processor.Array[i, j] != 0;
                  //  }
                }
            }
        }

        private void ClearArray()
        {
            for (int i = 0; i < xDimension; i++)
            {
                for (int j = 0; j < yDimension; j++)
                {
                    array[i, j].Fill = new SolidColorBrush(colorsArray[0]);
                }
            }
        }

        private void RefreshArray()
        {
            foreach (var cell in processor.UpdatedCells)
            {
                array[cell.X, cell.Y].Fill = new SolidColorBrush(colorsArray[processor.Array[cell.X, cell.Y]]);
            }
        }

        private async void stepButton_Click(object sender, RoutedEventArgs e)
        {
            if (!backgroundWorker.IsBusy)
            {
                backgroundWorker.RunWorkerAsync();
            }
        }

        private async void randomButton_Click(object sender, RoutedEventArgs e)
        {
            InitializeColorsArray(Convert.ToInt32(randomTextBox.Text));

            await processor.AddRandomGrains(Convert.ToInt32(randomTextBox.Text));

            RefreshFullArray();
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int x = 90;
            xTextBox.Dispatcher.Invoke(DispatcherPriority.Normal,
                (ThreadStart) delegate { x = Convert.ToInt32(this.xTextBox.Text); });
            processor.MakeStep(x);
        }

        private async void clearingbackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            await processor.Clear();
            renderingArray = new bool[xDimension, yDimension];
            await Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {
                ClearArray();
            }));
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RefreshFullArray();
        }

        private void radioButton2_Checked(object sender, RoutedEventArgs e)
        {
            processor.SetNeighbourhood(Neighbourhood.Moore);
        }

        private void radioButton3_Checked(object sender, RoutedEventArgs e)
        {
            processor.SetBorderStyle(Library.Models.BorderStyle.Closed);
        }

        private void radioButton1_Checked(object sender, RoutedEventArgs e)
        {
            processor.SetNeighbourhood(Neighbourhood.VonNeumann);
        }

        private void radioButton4_Checked(object sender, RoutedEventArgs e)
        {
            processor.SetBorderStyle(Library.Models.BorderStyle.Periodic);
        }

        private void radioButton5_Checked(object sender, RoutedEventArgs e)
        {
            processor.SetNeighbourhood(Neighbourhood.ShapeControl);
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            if (!clearingBackgroundWorker.IsBusy)
            {
                clearingBackgroundWorker.RunWorkerAsync();
            }
        }

        private async void importButton_Click(object sender, RoutedEventArgs e)
        {
            var openingHandler = new OpeningHandler("Text files (*.txt)|*.txt|All files (*.*)|*.*");
            var path = openingHandler.GetPath();

            if (path == null)
            {
                return;
            }

            TextHandler.ImportFromTextFile(processor.Array, xDimension, yDimension, path);

            await Task.Run(() =>
            {
                renderingArray = new bool[xDimension, yDimension];
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    ClearArray();
                    InitializeColorsArray(processor.Array.Max());
                    RefreshFullArray();
                }));
            });
        }

        private void exportButton_Click(object sender, RoutedEventArgs e)
        {
            var savingHandler = new SavingHandler("Text files (*.txt)|*.txt|All files (*.*)|*.*");
            var path = savingHandler.GetPath();

            if (path == null)
            {
                return;
            }

            TextHandler.ExportToTextFile(processor.Array, xDimension, yDimension, path);
        }

        private void exportImageButton_Click(object sender, RoutedEventArgs e)
        {
            var savingHandler = new SavingHandler("Png files (*.png)|*.png|All files (*.*)|*.*");
            var path = savingHandler.GetPath();

            if (path == null)
            {
                return;
            }

            ImageHandler.ExportToImage(canvas, path);
        }

        private async void importImageButton_Click(object sender, RoutedEventArgs e)
        {
            var openingHandler = new OpeningHandler("Png files (*.png)|*.png|All files (*.*)|*.*");
            var path = openingHandler.GetPath();

            if (path == null)
            {
                return;
            }

            ImageHandler.ImportFromImage(processor.Array, xDimension, yDimension, 600, path);

            await Task.Run(() =>
            {
                renderingArray = new bool[xDimension, yDimension];
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    ClearArray();
                    InitializeColorsArray(processor.Array.Max());
                    RefreshFullArray();
                }));
            });
        }

        private async void inclusionsButton_Click(object sender, RoutedEventArgs e)
        {
            await processor.AddInclusions(Convert.ToInt32(incusionsNumberField.Text), Convert.ToInt32(inclusionsSizeField.Text),
                (Inclusions)inclusionsComboBox.SelectedItem);

            await Task.Run(() =>
            {
                renderingArray = new bool[xDimension, yDimension];
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        renderingArray = new bool[xDimension, yDimension];
                        RefreshFullArray();
                    }));
                }));
            });       
        }

        private void canvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var xRatio = this.canvas.Width / xDimension;
            var yRatio = this.canvas.Height / yDimension; 
            var startPoint = e.GetPosition(canvas);
            this.coordinatesTextBox.Text = "X: " + (int)(startPoint.X / xRatio) + " Y: " + (int)(startPoint.Y / yRatio);
        }

        private void substructuresButton_Click(object sender, RoutedEventArgs e)
        {
            this.processor.CreateSubstructure((Substructures)substructuresComboBox.SelectedItem, Convert.ToInt32(substructuresTextBox.Text));
           // this.processor.ResetGrowth();

            renderingArray = new bool[xDimension, yDimension];
            ClearArray();
            RefreshFullArray();
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (dispatcherTimer.IsEnabled)
            {
                startButton.Content = "Start";
                dispatcherTimer.Stop();
            }
            else
            {
                startButton.Content = "Stop";
                dispatcherTimer.Start();
            }
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (!backgroundWorker.IsBusy)
            {
                backgroundWorker.RunWorkerAsync();
            }
        }
    }
}
