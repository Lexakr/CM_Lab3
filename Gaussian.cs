using System;

namespace CM_Lab3
{
    public static class Gaussian
    {
        /// <summary>
        /// Метод Гаусса с полным выбором главного элемента
        /// </summary>
        public static double[] GaussianEliminationFullPivot(double[,] matrix)
        {
            int n = matrix.GetLength(0);
            double[] result = new double[n];
            int[] columnOrder = new int[n];
            double[,] extendedMatrix = (double[,])matrix.Clone();

            for (int i = 0; i < n; i++)
            {
                columnOrder[i] = i;
            }

            // Прямой ход с выбором главного элемента по всей подматрице
            for (int k = 0; k < n - 1; k++)
            {
                // Находим индексы строки и столбца с максимальным элементом в подматрице
                int maxRowIndex = k;
                int maxColIndex = k;
                for (int i = k; i < n; i++)
                {
                    for (int j = k; j < n; j++)
                    {
                        if (Math.Abs(extendedMatrix[i, j]) > Math.Abs(extendedMatrix[maxRowIndex, maxColIndex]))
                        {
                            maxRowIndex = i;
                            maxColIndex = j;
                        }
                    }
                }

                // Меняем строки местами
                if (maxRowIndex != k)
                {
                    SwapRows(extendedMatrix, k, maxRowIndex);
                }

                // Меняем столбцы местами
                if (maxColIndex != k)
                {
                    SwapColumns(extendedMatrix, k, maxColIndex);
                    int tempIndex = columnOrder[k];
                    columnOrder[k] = columnOrder[maxColIndex];
                    columnOrder[maxColIndex] = tempIndex;
                }

                // Исключение переменных
                for (int i = k + 1; i < n; i++)
                {
                    double factor = extendedMatrix[i, k] / extendedMatrix[k, k];
                    for (int j = k + 1; j <= n; j++)
                    {
                        extendedMatrix[i, j] -= factor * extendedMatrix[k, j];
                    }
                }
            }

            // Обратный ход - вычисление значений переменных
            for (int i = n - 1; i >= 0; i--)
            {
                result[columnOrder[i]] = extendedMatrix[i, n];
                for (int j = i + 1; j < n; j++)
                {
                    result[columnOrder[i]] -= extendedMatrix[i, j] * result[columnOrder[j]];
                }
                result[columnOrder[i]] /= extendedMatrix[i, i];
            }

            return result;
        }


        // Метод для обмена строками matrix[row1] и matrix[row2]
        public static void SwapRows(double[,] matrix, int row1, int row2)
        {
            int n = matrix.GetLength(1);
            for (int i = 0; i < n; i++)
            {
                double temp = matrix[row1, i];
                matrix[row1, i] = matrix[row2, i];
                matrix[row2, i] = temp;
            }
        }

        // Метод для обмена столбцами matrix[:, col1] и matrix[:, col2]
        public static void SwapColumns(double[,] matrix, int col1, int col2)
        {
            int n = matrix.GetLength(0);
            for (int i = 0; i < n; i++)
            {
                double temp = matrix[i, col1];
                matrix[i, col1] = matrix[i, col2];
                matrix[i, col2] = temp;
            }
        }
    }
}
