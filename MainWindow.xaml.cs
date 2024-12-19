using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Converters;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace practice
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int Width = 800;
        private const int Height = 400;
        private const double MaxTemperature = 80;
        private const double MinTemperature = 30;
        private const double TemperatureRange = MaxTemperature - MinTemperature;
        private Random _random = new Random();
        private double _cpuTemperature = MinTemperature;
        private double _gpuTemperature = MinTemperature;
        private List<double> _cpuTemperatures = new List<double>();
        private List<double> _gpuTemperatures = new List<double>();
        private int _maxPoints = 50; 

        public MainWindow()
        {
            InitializeComponent();
            StartTeemperature(); 
        }

        private void StartTeemperature()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1); 
            timer.Tick += UpdateTemperatures;
            timer.Start();
        }


        private void UpdateTemperatures(object sender, EventArgs e)
        {
            if (!_isRunning) return;

            double cpuLoad = _random.Next(0, 100); 
            double gpuLoad = _random.Next(0, 100);

            UpdateTemperature(ref _cpuTemperature, cpuLoad);
            UpdateTemperature(ref _gpuTemperature, gpuLoad);

            if (_cpuTemperatures.Count >= _maxPoints)
            {
                _cpuTemperatures.RemoveAt(0);
            }
            _cpuTemperatures.Add(_cpuTemperature);

            if (_gpuTemperatures.Count >= _maxPoints)
            {
                _gpuTemperatures.RemoveAt(0);
            }
            _gpuTemperatures.Add(_gpuTemperature);

            DrawGraph();
        }

        private void UpdateTemperature(ref double temperature, double load)
        {
            if (load > 50)
            {
                temperature = Math.Min(MaxTemperature, temperature + 1); 
            }
            else
            {
                temperature = Math.Max(MinTemperature, temperature - 1); 
            }
        }

        private void DrawGraph()
        {
            Cv_Temprature.Children.Clear();
            DrawAxis();

            if (_cpuTemperatures.Count > 0)
            {
                DrawTemperatureGraph(_cpuTemperatures, Brushes.Blue, "CPU");
            }
            if (_gpuTemperatures.Count > 0)
            {
                DrawTemperatureGraph(_gpuTemperatures, Brushes.Red, "GPU");
            }

            StatusTextBlock.Text = $"CPU: {_cpuTemperature:0}°C, GPU: {_gpuTemperature:0}°C";
        }

        private void DrawAxis()
        {
            // Ось Y
            Line yAxis = new Line
            {
                X1 = 50,
                Y1 = 20,
                X2 = 50,
                Y2 = Height - 20,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            Cv_Temprature.Children.Add(yAxis);

            // Ось X
            Line xAxis = new Line
            {
                X1 = 50,
                Y1 = Height - 20,
                X2 = Width - 20,
                Y2 = Height - 20,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            Cv_Temprature.Children.Add(xAxis);

            for (int temp = (int)MinTemperature; temp <= (int)MaxTemperature; temp += 10)
            {
                double normalizedY = ((MaxTemperature - temp) / TemperatureRange) * (Height - 60) + 20; 
               
                Line tick = new Line
                {
                    X1 = 45, 
                    Y1 = normalizedY,
                    X2 = 55, 
                    Y2 = normalizedY,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                Cv_Temprature.Children.Add(tick);

                TextBlock tempLabel = new TextBlock
                {
                    Text = $"{temp}°C",
                    FontSize = 12,
                    Foreground = Brushes.Black,
                    Margin = new Thickness(5, normalizedY - 10, 0, 0)
                };

                Cv_Temprature.Children.Add(tempLabel); 

                for (int i = 1; i <= 9; i++)
                {
                    int intermediateTemp = temp + i; 
                    if (intermediateTemp <= MaxTemperature) 
                    {
                        double intermediateY = ((MaxTemperature - intermediateTemp) / TemperatureRange) * (Height - 60) + 20; // Нормализуем

                        Line intermediateTick = new Line
                        {
                            X1 = 50,
                            Y1 = intermediateY,
                            X2 = 60,
                            Y2 = intermediateY,
                            Stroke = Brushes.Black,
                            StrokeThickness = 1
                        };
                        Cv_Temprature.Children.Add(intermediateTick);
                    }
                }
            }
        }

        private void DrawTemperatureGraph(List<double> temperatures, Brush color, string label)
        {
            if (temperatures.Count < 2) return;
            for (int i = 0; i < temperatures.Count - 1; i++)
            {
                double normalizedTemp1 = ((MaxTemperature - temperatures[i]) / TemperatureRange) * (Height - 60) + 20; 
                double normalizedTemp2 = ((MaxTemperature - temperatures[i + 1]) / TemperatureRange) * (Height - 60) + 20; 

                Line line = new Line
                {
                    X1 = 50 + (i * (Width - 100) / (_maxPoints - 1)),
                    Y1 = normalizedTemp1,
                    X2 = 50 + ((i + 1) * (Width - 100) / (_maxPoints - 1)),
                    Y2 = normalizedTemp2,
                    Stroke = color,
                    StrokeThickness = 2
                };
                Cv_Temprature.Children.Add(line);

                Ellipse point = new Ellipse
                {
                    Fill = color,
                    Width = 5,
                    Height = 5
                };
                Canvas.SetLeft(point, 50 + (i * (Width - 100) / (_maxPoints - 1)) - 2.5);
                Canvas.SetTop(point, normalizedTemp1 - 2.5);
                Cv_Temprature.Children.Add(point);
            }
        }
        private bool _isRunning = true;

        private void btn_Stop_Click(object sender, RoutedEventArgs e)
        {
            _isRunning = false;
            btn_Stop.IsEnabled = false;
            btn_Continue.IsEnabled = true;

            StatusTextBlock.Text = "Остановлено";
        }

        private void btn_Continue_Click(object sender, RoutedEventArgs e)
        {
            _isRunning = true;
            StartTeemperature();
            btn_Continue.IsEnabled = false;
            btn_Stop.IsEnabled = true;
        }

        private void btn_Reset_Click(object sender, RoutedEventArgs e)
        {
            _cpuTemperature = MinTemperature;
            _gpuTemperature = MinTemperature;
            _cpuTemperatures.Clear();
            _gpuTemperatures.Clear();

            Cv_Temprature.Children.Clear();
            DrawAxis();

            StatusTextBlock.Text = $"CPU: {_cpuTemperature:0}°C, GPU: {_gpuTemperature:0}°C";
        }

        private void Cv_Temprature_MouseMove(object sender, MouseEventArgs e)
        {
            Point position = e.GetPosition(Cv_Temprature);
            double mouseX = position.X;

            int index = (int)((mouseX - 50) / ((Width - 100) / (_maxPoints - 1)));
            if (index >= 0 && index < _cpuTemperatures.Count)
            {
                double cpuTemp = _cpuTemperatures[index];
                double gpuTemp = _gpuTemperatures[index];

                StatusTextBlock.Text = $"CPU: {cpuTemp:0}°C, GPU: {gpuTemp:0}°C";
            }
        }
    }
}