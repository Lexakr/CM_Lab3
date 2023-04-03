using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using static CM_Lab3.Approximations;

namespace CM_Lab3
{
    public partial class MainWindow : Window
    {
        private readonly PlotModel _plotModel;
        private LineSeries lagrangeSeries;
        private LineSeries newtonSeries;
        private LineSeries polynomialSeries;
        private bool IsSmoothedSeries = false;

        private double[] coefficients;
        private double minX;
        private double maxX;

        public MainWindow()
        {
            InitializeComponent();

            _plotModel = new PlotModel
            {
                Title = "Аппроксимация функции",
                Axes =
                {
                    new LinearAxis
                    {
                        Position = AxisPosition.Bottom,
                        PositionAtZeroCrossing = true,
                        AxislineStyle = LineStyle.Solid,
                        TickStyle = TickStyle.Crossing,
                        MajorGridlineStyle = LineStyle.Solid,
                        MinorGridlineStyle = LineStyle.Dot
                    },
                    new LinearAxis
                    {
                        Position = AxisPosition.Left,
                        PositionAtZeroCrossing = true,
                        AxislineStyle = LineStyle.Solid,
                        TickStyle = TickStyle.Crossing,
                        MajorGridlineStyle = LineStyle.Solid,
                        MinorGridlineStyle = LineStyle.Dot
                    }
                }
            };

            PlotView.Model = _plotModel;
        }

        private void CreateSeriesButton_Click(object sender, RoutedEventArgs e)
        {
            // Создаем OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
            List<DataPoint> points;

            // Открываем диалоговое окно выбора файла
            if (openFileDialog.ShowDialog() == true)
            {
                // Получаем имя выбранного файла
                var fileName = openFileDialog.FileName;
                // Загрузка точек для графика из файла
                points = ReadPointsFromFile(fileName);
            }
            else
            {
                return;
            } 

            EnableButtons();
            // Очистка графиков перед построением новых
            _plotModel.Series.Clear();;

            List<ScatterPoint> scatterPoints = points.ConvertAll(dp => new ScatterPoint(dp.X, dp.Y, 5, 0));

            var step = 0.01;
            // Область кусочно-линейной аппроксимации
            minX = points.Min(p => p.X);
            maxX = points.Max(p => p.X);

            // График интерполяции многочленом Лагранжа
            var lagrangePoints = new List<DataPoint>();
            for (double x = minX; x <= maxX; x += step)
            {
                double y = LagrangeInterpolation(points, x);
                lagrangePoints.Add(new DataPoint(x, y));
            }
            lagrangeSeries = new LineSeries
            {
                Title = "Интерполяция Лагранжа",
                LineStyle = LineStyle.Solid,
                Color = OxyColors.Blue,
                StrokeThickness = 5
            };
            lagrangeSeries.Points.AddRange(lagrangePoints);
            _plotModel.Series.Add(lagrangeSeries);

            // График интерполяции многочленом Ньютона
            var newtonPoints = new List<DataPoint>();
            for (double x = minX; x <= maxX; x += step)
            {
                double y = NewtonInterpolation(scatterPoints, x);
                newtonPoints.Add(new DataPoint(x, y));
            }
            newtonSeries = new LineSeries
            {
                Title = "Интерполяция Ньютона",
                LineStyle = LineStyle.Solid,
                Color = OxyColors.Red
            };
            newtonSeries.Points.AddRange(newtonPoints);
            _plotModel.Series.Add(newtonSeries);

            // Графики сглаживающих многочленов 1..3 степени
            for (int degree = 1; degree <= 3; degree++)
            {
                var coefficients = FindPolynomialCoefficients(scatterPoints, degree);
                var smoothedPoints = new List<DataPoint>();

                for (double x = minX; x <= maxX; x += step)
                {
                    double y = EvaluatePolynomial(coefficients, x);
                    smoothedPoints.Add(new DataPoint(x, y));
                }

                var smoothedSeries = new LineSeries { Title = $"Сглаживающий многочлен {degree} степени" };
                smoothedSeries.Points.AddRange(smoothedPoints);
                _plotModel.Series.Add(smoothedSeries);
            }
            IsSmoothedSeries = true;

            // График исходных точек
            var scatterSeries = new ScatterSeries { Title = "Исходные точки" };
            scatterSeries.Points.AddRange(scatterPoints);
            _plotModel.Series.Add(scatterSeries);

            _plotModel.IsLegendVisible = true;
            // Добавляем легенду к графику
            _plotModel.Legends.Add(new Legend()
            {
                LegendPlacement = LegendPlacement.Outside,

            });
            _plotModel.InvalidatePlot(true);
        }

        private void LagrangeButton_Click(object sender, RoutedEventArgs e)
        {
            if (lagrangeSeries == null) return;
            if (_plotModel.Series[0].IsVisible)
            {
                _plotModel.Series[0].IsVisible = false;
            }
            else
            {
                _plotModel.Series[0].IsVisible = true;
            }

            _plotModel.InvalidatePlot(true);
        }

        private void NewtonButton_Click(object sender, RoutedEventArgs e)
        {
            if (newtonSeries == null) return;
            if (_plotModel.Series[1].IsVisible)
            {
                _plotModel.Series[1].IsVisible = false;
            }
            else
            {
                _plotModel.Series[1].IsVisible = true;
            }

            _plotModel.InvalidatePlot(true);
        }

        private void PolyButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsSmoothedSeries) return;
            for (int i = 2; i <= 4; i++)
            {
                if (_plotModel.Series[i].IsVisible)
                {
                    _plotModel.Series[i].IsVisible = false;
                }
                else
                {
                    _plotModel.Series[i].IsVisible = true;
                }

                _plotModel.InvalidatePlot(true);
            }

        }

        private void CoefficientButton_Click(object sender, RoutedEventArgs e)
        {
            // Создаем OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";

            // Открываем диалоговое окно выбора файла
            if (openFileDialog.ShowDialog() == true)
            {
                // Получаем имя выбранного файла
                string fileName = openFileDialog.FileName;

                // Вызываем метод и передаем ему имя файла
                coefficients = ReadCoefficientsFromFile(fileName);
            }
        }

        private void CoefficientChartButton_Click(object sender, RoutedEventArgs e)
        {
            if (coefficients == null) return;

            if (polynomialSeries != null) _plotModel.Series.Remove(polynomialSeries);

            List<DataPoint> polynomialPoints = GenerateCustomPolynomialPoints(coefficients, minX, maxX);

            // Создаем объект LineSeries для графика многочлена
            polynomialSeries = new LineSeries
            {
                Title = "Многочлен заданных коэффициентов",
                StrokeThickness = 2,
                LineStyle = LineStyle.Dot,
                Color = OxyColors.White
            };

            // Добавляем точки к графику многочлена
            polynomialSeries.Points.AddRange(polynomialPoints);

            // Добавляем график многочлена на график
            _plotModel.Series.Add(polynomialSeries);

            _plotModel.InvalidatePlot(true);
        }

        private void CoefficientShowButton_Click(object sender, RoutedEventArgs e)
        {
            if (polynomialSeries == null) return;
            if (polynomialSeries.IsVisible)
            {
                polynomialSeries.IsVisible = false;
            }
            else
            {
                polynomialSeries.IsVisible = true;
            }

            _plotModel.InvalidatePlot(true);
        }

        private static double[] ReadCoefficientsFromFile(string fileName)
        {
            // Создание объекта для чтения данных из файла
            StreamReader reader = new StreamReader(fileName);
            string line = reader.ReadLine();
            reader.Close();

            string[] substrings = line.Split(' ');

            double[] numbers = new double[substrings.Length];

            for (int i = 0; i < substrings.Length; i++)
            {
                numbers[i] = Double.Parse(substrings[i]);
            }

            return numbers;
        }

        private static List<DataPoint> ReadPointsFromFile(string fileName)
        {
            string[] lines = File.ReadAllLines(fileName);

            List<DataPoint> points = new();

            for (int i = 0; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split(' ');
                DataPoint dot = new(double.Parse(parts[0]), double.Parse(parts[1]));
                points.Add(dot);
            }

            return points;
        }

        private void EnableButtons()
        {
            if (!LagrangeButton.IsEnabled) LagrangeButton.IsEnabled = true;
            if (!NewtonButton.IsEnabled) NewtonButton.IsEnabled = true;
            if (!PolyButton.IsEnabled) PolyButton.IsEnabled = true;
            if (!CoefficientButton.IsEnabled) CoefficientButton.IsEnabled = true;
            if (!CoefficientChartButton.IsEnabled) CoefficientChartButton.IsEnabled = true;
            if (!CoefficientShowButton.IsEnabled) CoefficientShowButton.IsEnabled = true;
        }
    }
}
