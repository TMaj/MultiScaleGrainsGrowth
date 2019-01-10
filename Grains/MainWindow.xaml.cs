using Grains.Library.Enums;
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
        private int xDimension;
        private int yDimension;
        private int matrixIdsNumber;
        private bool microstructureDisplayed;
        private SimulationType currentChosenSimulation = SimulationType.CellularAutomata;

        private readonly BackgroundWorker backgroundWorker;
        private readonly BackgroundWorker clearingBackgroundWorker;
        private readonly DispatcherTimer dispatcherTimer;

        public MainWindow()
        {
            xDimension = 300;
            yDimension = 300;
            microstructureDisplayed = true;

            array = new Rectangle[xDimension, yDimension];
            processor = new Processor(xDimension, yDimension);
            matrixIdsNumber = processor.IdsNumber;
            colorsArray = new Color[0];
            backgroundWorker = new BackgroundWorker();
            clearingBackgroundWorker = new BackgroundWorker();

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);

            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += worker_RunWorkerCompleted;

            clearingBackgroundWorker.DoWork += clearingbackgroundWorker_DoWork;

            processor.StepIncremented += UpdateCurrentRXStep;

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

        private Color GetRandomColor(Random random, bool redColor = false)
        {
            var color = new Color();
            color.R = (byte)random.Next(0, 255);
            color.A = 255;

            if (redColor)
            {
                color.G = 0;
                color.B = 0;
            }
            else
            {
                color.G = (byte)random.Next(0, 255);
                color.B = (byte)random.Next(0, 255);
            }
                     
            return color;
        }

        private void InitializeColorsArray(int value, bool oneColorType = false)
        {
            var rand = new Random();

            if (colorsArray != null && colorsArray.Count() != 0)
            {
                if (this.processor.Array.Max() + 2 <= colorsArray.Length)
                {
                    return;
                }

                var oldColors = colorsArray;

                colorsArray = new Color[oldColors.Count() + value];

                for (int i = 0; i < oldColors.Count(); i++)
                {
                    colorsArray[i] = oldColors[i];
                }

                for (int i = oldColors.Count() - 1; i < oldColors.Count() + value; i++)
                {
                   colorsArray[i] = GetRandomColor(rand, oneColorType);
                }
         
            }
            else
            {
                colorsArray = new Color[value + 2];
                colorsArray[0] = Colors.White;
                colorsArray[1] = Colors.Black;

                for (int i = 2; i < value + 2; i++)
                {
                    colorsArray[i] = GetRandomColor(rand, oneColorType);
                }
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
            for (int i = 0; i < xDimension; i++)
            {
                for (int j = 0; j < yDimension; j++)
                {
                    if (processor.Array[i, j] == 0)
                    {
                        continue;
                    }

                    if (array[i, j].Fill.ToString().Equals(colorsArray[processor.Array[i, j]].ToString()))
                    {
                        continue;
                    }

                    array[i, j].Fill = new SolidColorBrush(colorsArray[processor.Array[i, j]]);
                }
            }
        }

        private void DisplayEnergyArray()
        {
            for (int i = 0; i < xDimension; i++)
            {
                for (int j = 0; j < yDimension; j++)
                {
                    var color = new Color();
                    color.R = (byte)(255 / (processor.Energy[i, j] +1));
                    color.G = (byte)(255 / (processor.Energy[i, j] +1));
                    color.B = (byte)(255 / (processor.Energy[i, j] + 1));
                    color.A = 255;
                    array[i, j].Fill = new SolidColorBrush(color);
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

        private async void stepButton_Click(object sender, RoutedEventArgs e)
        {
            if (!backgroundWorker.IsBusy)
            {
                backgroundWorker.RunWorkerAsync();
            }
        }

        private async void randomButton_Click(object sender, RoutedEventArgs e)
        {
            await processor.AddRandomGrains(Convert.ToInt32(randomTextBox.Text));

            InitializeColorsArray(processor.IdsNumber);

            RefreshFullArray();
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int x = 90;
            double j = 0.1;
            matrixIdsNumber = processor.IdsNumber;

            xTextBox.Dispatcher.Invoke(DispatcherPriority.Normal,
                (ThreadStart)delegate { x = Convert.ToInt32(this.xTextBox.Text); });
            jgbTextBox.Dispatcher.Invoke(DispatcherPriority.Normal,
                (ThreadStart)delegate { j = double.Parse(jgbTextBox.Text, System.Globalization.CultureInfo.InvariantCulture); });

            processor.MakeStep(x, j);
        }

        private async void clearingbackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            await processor.Clear();
            await Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {
                this.colorsArray = new Color[0];
                InitializeColorsArray(0);
                ClearArray();
            }));

            Dispatcher.Invoke(() => loadingStackPanel.Visibility = Visibility.Collapsed);
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (matrixIdsNumber < processor.IdsNumber)
            {
                InitializeColorsArray(processor.IdsNumber - matrixIdsNumber, true);
                matrixIdsNumber = processor.IdsNumber;
            }
            RefreshFullArray();
        }

        private void radioButton2_Checked(object sender, RoutedEventArgs e)
        {
            processor.Neighbourhood = Neighbourhood.Moore;
        }

        private void radioButton3_Checked(object sender, RoutedEventArgs e)
        {
            processor.SetBorderStyle(Library.Models.BorderStyle.Closed);
        }

        private void radioButton1_Checked(object sender, RoutedEventArgs e)
        {
            processor.Neighbourhood = Neighbourhood.VonNeumann;
        }

        private void radioButton4_Checked(object sender, RoutedEventArgs e)
        {
            processor.SetBorderStyle(Library.Models.BorderStyle.Periodic);
        }

        private void radioButton5_Checked(object sender, RoutedEventArgs e)
        {
            processor.Neighbourhood = Neighbourhood.ShapeControl;
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            if (!clearingBackgroundWorker.IsBusy)
            {
                currentRXStepTextBlock.Text = "0";
                energyDistributionCheckBox.IsChecked = false;
                clearingBackgroundWorker.RunWorkerAsync();
                loadingStackPanel.Visibility = Visibility.Visible;
            }
        }

        private async void importButton_Click(object sender, RoutedEventArgs e)
        {
            var openingHandler = new OpeningHandler("Text files (*.txt)|*.txt|All files (*.*)|*.*");
            var path = openingHandler.GetPath();

            if (path == null || path == "")
            {
                return;
            }

            await RunLongTask(TextHandler.ImportFromTextFile(processor.Array, xDimension, yDimension, path),
                () => {
                    ClearArray();
                    InitializeColorsArray(processor.Array.Max());
                    RefreshFullArray();
                });
        }

        private void exportButton_Click(object sender, RoutedEventArgs e)
        {
            var savingHandler = new SavingHandler("Text files (*.txt)|*.txt|All files (*.*)|*.*");
            var path = savingHandler.GetPath();

            if (path == null || path == "")
            {
                return;
            }

            TextHandler.ExportToTextFile(processor.Array, xDimension, yDimension, path);
        }

        private void exportImageButton_Click(object sender, RoutedEventArgs e)
        {
            var savingHandler = new SavingHandler("Png files (*.png)|*.png|All files (*.*)|*.*");
            var path = savingHandler.GetPath();

            if (path == null || path == "")
            {
                return;
            }

            ImageHandler.ExportToImage(canvas, path);
        }

        private async void importImageButton_Click(object sender, RoutedEventArgs e)
        {
            var openingHandler = new OpeningHandler("Png files (*.png)|*.png|All files (*.*)|*.*");
            var path = openingHandler.GetPath();

            if (path == null || path == "")
            {
                return;
            }

            await RunLongTask(ImageHandler.ImportFromImage(processor.Array, xDimension, yDimension, 600, path),
                () => {
                    ClearArray();
                    InitializeColorsArray(processor.Array.Max());
                    RefreshFullArray();
                });
        }

        private async void inclusionsButton_Click(object sender, RoutedEventArgs e)
        {
            await RunLongTask(processor.AddInclusions(Convert.ToInt32(incusionsNumberField.Text),
                                                      Convert.ToInt32(inclusionsSizeField.Text),
                                                      (Inclusions)inclusionsComboBox.SelectedItem),
                            () => {
                                RefreshFullArray();
                            });
        }

        private async void canvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var xRatio = this.canvas.Width / xDimension;
            var yRatio = this.canvas.Height / yDimension;
            var clickPoint = e.GetPosition(canvas);

            var clickedX = (int)(clickPoint.X / xRatio);
            var clickedY = (int)(clickPoint.Y / yRatio);

            this.coordinatesTextBox.Text = "X: " + clickedX + " Y: " + clickedY;

            if ((bool)bordersCheckbox.IsChecked)
            {
                await RunLongTask(processor.AddSingleBorder(Convert.ToInt32(bordersTextBox.Text), clickedX, clickedY),
                    () => {
                        SetBordersPercentage();
                        RefreshFullArray();
                    });
            }
        }

        private async void substructuresButton_Click(object sender, RoutedEventArgs e)
        {
            await RunLongTask(processor.CreateSubstructure((Substructures)substructuresComboBox.SelectedItem, Convert.ToInt32(substructuresTextBox.Text)),
                () => {
                    ClearArray();
                    RefreshFullArray();
                });
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (dispatcherTimer.IsEnabled)
            {
                startButton.Content = "Start";
                dispatcherTimer.Stop();
                rxButton.IsEnabled = true;
            }
            else
            {
                startButton.Content = "Stop";
                dispatcherTimer.Start();
                rxButton.IsEnabled = false;
            }
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (processor.SimulationType == SimulationType.RXMonteCarlo && (bool)stepsLimitCheckBox.IsChecked && processor.CurrentRXStep >= processor.StepsLimit)
            {
                processor.SimulationType = currentChosenSimulation;
                startButton.IsEnabled = true;
                rxButton.Content = "Start RX";
                dispatcherTimer.Stop();
            }

            if (!backgroundWorker.IsBusy)
            {
                backgroundWorker.RunWorkerAsync();
            }
        }

        private async void createBordersButton_Click(object sender, RoutedEventArgs e)
        {
            await RunLongTask(processor.AddBorders(Convert.ToInt32(bordersTextBox.Text)), (() =>
            {
                RefreshFullArray();
                SetBordersPercentage();
                loadingStackPanel.Visibility = Visibility.Collapsed;
            }));
        }

        private async void clearBackgroundButton_Click(object sender, RoutedEventArgs e)
        {
            await RunLongTask(processor.ClearAllButBorders(),
                () =>
                {
                    ClearArray();
                    RefreshFullArray();
                });
        }

        private async void SetBordersPercentage()
        {
            percentageLabel.Content = String.Format("{0:F2}", await processor.GetBordersPercentage());
        }

        private async void McGenerationButton_Click(object sender, RoutedEventArgs e)
        {
            await RunLongTask(this.processor.GenerateMonteCarloArea(Convert.ToInt32(mcStatesTextBox.Text)),
                () => {
                    InitializeColorsArray(Convert.ToInt32(mcStatesTextBox.Text));
                    RefreshFullArray();
                });
        }

        private async Task RunLongTask(Task longTask, Action afterCompleted)
        {
            var longOperation = longTask.ContinueWith((task) => {
                Dispatcher.Invoke((Action)(() =>
                {
                    loadingStackPanel.Visibility = Visibility.Visible;
                    afterCompleted();
                    loadingStackPanel.Visibility = Visibility.Collapsed;
                }));
            });

            loadingStackPanel.Visibility = Visibility.Visible;

            await longOperation;
        }

        private void SimCARadioButton_Checked(object sender, RoutedEventArgs e)
        {
            this.processor.SimulationType = SimulationType.CellularAutomata;
            currentChosenSimulation = SimulationType.CellularAutomata;
        }

        private void SimMCRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            this.processor.SimulationType = SimulationType.MonteCarlo;
            currentChosenSimulation = SimulationType.MonteCarlo;
        }

        private void JgbTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (double.Parse(jgbTextBox.Text, System.Globalization.CultureInfo.InvariantCulture) < 0.1)
            {
                jgbTextBox.Text = "0.1";
            }

            if (double.Parse(jgbTextBox.Text, System.Globalization.CultureInfo.InvariantCulture) > 1)
            {
                jgbTextBox.Text = "1";
            }
        }

        private async void DistributeEnergButton_Click(object sender, RoutedEventArgs e)
        {
            EnergyDistributionType energyDistributionType;
            if ((bool)heterogenousRadioButton.IsChecked)
            {
                energyDistributionType = EnergyDistributionType.Heterogenous;
            }
            else
            {
                energyDistributionType = EnergyDistributionType.Homogenous;
            }
            await RunLongTask(this.processor.DistributeEnergy(energyDistributionType), () => {
                if (!microstructureDisplayed)
                {
                    DisplayEnergyArray();                   
                }
                energyDistributionCheckBox.IsChecked = true;
            });
        }

        private void ShowEnergyButton_Click(object sender, RoutedEventArgs e)
        {
            if (microstructureDisplayed)
            {
                    DisplayEnergyArray();
                    this.showEnergyButton.Content = "Show microstructure";
                    lowLabel.Visibility = Visibility.Visible;
                    highLabel.Visibility = Visibility.Visible;
                    legendRectangle.Visibility = Visibility.Visible;
            }
            else
            {
                RefreshFullArray();
                this.showEnergyButton.Content = "Show energy";
                lowLabel.Visibility = Visibility.Hidden;
                highLabel.Visibility = Visibility.Hidden;
                legendRectangle.Visibility = Visibility.Hidden;
            }

            microstructureDisplayed = !microstructureDisplayed;
        }

        private void UpdateCurrentRXStep(int stepNumber)
        {
           Dispatcher.Invoke((Action)(() => { this.currentRXStepTextBlock.Text = stepNumber.ToString(); }));
        }

        private async void ClearEnergyButton_Click(object sender, RoutedEventArgs e)
        {
            await RunLongTask(processor.ClearEnergy(), () => {
                if (!microstructureDisplayed)
                {
                    DisplayEnergyArray();
                }
                energyDistributionCheckBox.IsChecked = false;
            });
        }

        private void RxButton_Click(object sender, RoutedEventArgs e)
        {

            if (dispatcherTimer.IsEnabled)
            {
                processor.SimulationType = currentChosenSimulation;
                startButton.IsEnabled = true;
                rxButton.Content = "Start RX";
                dispatcherTimer.Stop();
            }
            else
            {
                processor.SimulationType = SimulationType.RXMonteCarlo;
                startButton.IsEnabled = false;
                rxButton.Content = "Stop RX";
                dispatcherTimer.Start();
            }
        }

        private void ConstantRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            processor.NucleationType = NucleationModuleType.Constant;
            processor.NucleationSize = Convert.ToInt32(this.rxConstantNucleonsTextBox.Text);
            rxNucleonsButton.IsEnabled = false;
        }

        private void IncreasingRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            processor.NucleationType = NucleationModuleType.Increasing;
            processor.NucleationSize = Convert.ToInt32(this.rxIncreasingNucleonsTextBox.Text);
            rxNucleonsButton.IsEnabled = false;
        }

        private void BeginningConstRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            processor.NucleationType = NucleationModuleType.BeginningConstant;
            processor.NucleationSize = Convert.ToInt32(this.rxBeginningIncreasingNucleonsTextBox.Text);
            rxNucleonsButton.IsEnabled = true;
        }

        private void RxNucleonsGBradioButton_Checked(object sender, RoutedEventArgs e)
        {
            processor.NucleationArea = NucleationArea.GrainBoundaries;
        }

        private void RxNucleonsRandomradioButton_Checked(object sender, RoutedEventArgs e)
        {
            processor.NucleationArea = NucleationArea.Random;
        }

        private void StepsLimitTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            processor.StepsLimit = Convert.ToInt32(this.stepsLimitTextBox.Text);
        }

        private async void RxNucleonsButton_Click(object sender, RoutedEventArgs e)
        {
            await RunLongTask(this.processor.AddRecrystalisedNucleons(), () => {
                InitializeColorsArray(processor.NucleationSize, true);
                RefreshFullArray();
            });
        }

        private void RxBeginningIncreasingNucleonsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            processor.NucleationSize = Convert.ToInt32(this.rxBeginningIncreasingNucleonsTextBox.Text);
        }

        private void EnergyDistributionCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            rxButton.IsEnabled = true;
        }

        private void EnergyDistributionCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            rxButton.IsEnabled = false;
        }
    }
}
