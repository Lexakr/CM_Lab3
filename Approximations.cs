using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using static CM_Lab3.Gaussian;

namespace CM_Lab3
{
    public static class Approximations
    {
        public static double LagrangeInterpolation(IList<DataPoint> points, double x)
        {
            // Значение интерполяционного многочлена Лагранжа в точке x
            double result = 0.0;

            // Цикл по всем точкам, через которые проходит многочлен Лагранжа
            for (int i = 0; i < points.Count; i++)
            {
                // Значение базисного полинома Лагранжа в точке x
                double basis = 1.0;
                // Значения X и Y текущей точки
                double xi = points[i].X;
                double yi = points[i].Y;

                // Вложенный цикл для вычисления базисного полинома Лагранжа
                foreach (var point in points)
                {
                    // Если X вложенной точки не равен X текущей точки, выполняем умножение
                    if (point.X != xi)
                    {
                        basis *= (x - point.X) / (xi - point.X);
                    }
                }

                // Добавление значения базисного полинома Лагранжа, умноженного на значение функции в точке yi, к результату
                result += basis * yi;
            }

            return result;
        }

        /// <summary>
        /// Метод для вычисления разделенной разности для заданного интервала точек
        /// </summary>
        private static double DividedDifference(IList<ScatterPoint> points, int startIndex, int endIndex)
        {
            int n = points.Count;
            // Создание двумерного массива для хранения разделенных разностей
            double[,] dividedDifferences = new double[n, n];

            // Заполнение первого столбца массива значениями Y координат точек
            for (int i = 0; i < n; i++)
            {
                dividedDifferences[i, 0] = points[i].Y;
            }

            // Вычисление разделенных разностей и заполнение оставшихся столбцов массива
            for (int j = 1; j < n; j++)
            {
                for (int i = 0; i < n - j; i++)
                {
                    dividedDifferences[i, j] = (dividedDifferences[i + 1, j - 1] - dividedDifferences[i, j - 1]) / (points[i + j].X - points[i].X);
                }
            }

            return dividedDifferences[startIndex, endIndex - startIndex];
        }

        /// <summary>
        /// Метод для интерполяции функции с помощью формулы Ньютона
        /// </summary>
        public static double NewtonInterpolation(IList<ScatterPoint> points, double x)
        {
            // Начальное значение результата - значение Y координаты первой точки
            double result = points[0].Y;
            // Переменная для хранения произведения (x-x0)*(x-x1)*...*(x-xn)
            double xn = 1;

            // Цикл по точкам для вычисления интерполирующего полинома
            for (int n = 1; n < points.Count; n++)
            {
                // Обновление произведения (x-x0)*(x-x1)*...*(x-xn)
                xn *= (x - points[n - 1].X);
                // Прибавление значения i-го члена интерполирующего полинома к результату
                result += DividedDifference(points, 0, n) * xn;
            }

            return result;
        }

        /// <summary>
        /// Метод для расчета суммы произведений координат точек с заданными степенями
        /// </summary>
        private static double CalculateSum(List<ScatterPoint> points, int xPower, int yPower)
        {
            return points.Sum(point => Math.Pow(point.X, xPower) * Math.Pow(point.Y, yPower));
        }

        /// <summary>
        /// Метод для нахождения коэффициентов полинома заданной степени с помощью метода наименьших квадратов
        /// </summary>
        public static double[] FindPolynomialCoefficients(List<ScatterPoint> points, int degree)
        {
            // Количество коэффициентов равно степени полинома + 1
            int n = degree + 1;
            // Создание матрицы для хранения коэффициентов и свободных членов СЛАУ
            double[,] matrix = new double[n, n + 1];

            // Заполнение матрицы коэффициентов и свободных членов
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    // Заполнение матрицы коэффициентов суммами степеней X
                    matrix[i, j] = CalculateSum(points, i + j, 0);
                }
                // Заполнение столбца свободных членов суммами произведений координат точек на степени X
                matrix[i, n] = CalculateSum(points, i, 1);
            }

            // Решение СЛАУ (частичный выбор по столбцу) и возврат коэффициентов полинома
            return GaussianEliminationFullPivot(matrix);
        }

        /// <summary>
        /// Метод для вычисления значения полинома с заданными коэффициентами в точке x
        /// </summary>
        public static double EvaluatePolynomial(double[] coefficients, double x)
        {
            double result = 0;
            // Цикл по коэффициентам полинома
            for (int i = 0; i < coefficients.Length; i++)
            {
                // Суммирование произведений коэффициента на соответствующую степень X
                result += coefficients[i] * Math.Pow(x, i);
            }
            return result;
        }

        /// <summary>
        /// Метод вычисляет значение многочлена четвертой степени в точке X с заданными коэффициентами
        /// </summary>
        private static double PolynomialFunction(double[] coefficients, double x)
        {
            return coefficients[0] + coefficients[1] * x + coefficients[2] * Math.Pow(x, 2)
                + coefficients[3] * Math.Pow(x, 3) + coefficients[4] * Math.Pow(x, 4);
        }

        /// <summary>
        /// Метод, вычисляющий точки для построения графика по 
        /// многочлену четвертой степени с заданными коэффициентами
        /// </summary>
        public static List<DataPoint> GenerateCustomPolynomialPoints(double[] coefficients, double minX, double maxX)
        {
            // Создаем набор точек для графика многочлена
            List<DataPoint> polynomialPoints = new();

            // Вычисляем значения многочлена между minX и maxX с шагом
            double step = 0.01; // 10^-2
            for (double x = minX; x < maxX; x += step)
            {
                double y = PolynomialFunction(coefficients, x);
                polynomialPoints.Add(new DataPoint(x, y));
            }

            return polynomialPoints;
        }
    }
}
