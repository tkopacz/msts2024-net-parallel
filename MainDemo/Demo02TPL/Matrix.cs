using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo02TPL
{
    class DoubleMatrixOperations
    {
        public static Matrix<double> MultiplySequential(Matrix<double> m1, Matrix<double> m2)
        {
            Matrix<double> result = new Matrix<double>(m1.Rows, m2.Columns);
            for (int i = 0; i < m1.Rows; i++)
            {
                for (int j = 0; j < m2.Columns; j++)
                {
                    result[i, j] = 0;
                    for (int k = 0; k < m1.Columns; k++)
                    {
                        result[i, j] += m1[i, k] * m2[k, j];
                    }
                }
            }
            return result;
        }

        public static Matrix<double> MultiplyParallel(Matrix<double> m1, Matrix<double> m2)
        {
            Matrix<double> result = new Matrix<double>(m1.Rows, m2.Columns);
            Parallel.For(0, m1.Rows, i =>
            {
                for (int j = 0; j < m2.Columns; j++)
                {
                    result[i, j] = 0;
                    for (int k = 0; k < m1.Columns; k++)
                    {
                        result[i, j] += m1[i, k] * m2[k, j];
                    }
                }
            });
            return result;
        }

        public static Matrix<double> GenerateRandom(int rows, int cols)
        {
            Random rnd = new Random();
            Matrix<double> m = new Matrix<double>(rows, cols);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    m[i, j] = rnd.NextDouble();
                }
            }
            return m;
        }
    }

    class Matrix<T>
    {
        private T[,] _values;

        public Matrix(int rows, int columns)
        {
            if (rows < 1) throw new ArgumentOutOfRangeException("rows");
            if (columns < 1) throw new ArgumentOutOfRangeException("columns");
            Rows = rows;
            Columns = columns;
            _values = new T[rows, columns];
        }

        public T this[int row, int column]
        {
            get { return _values[row, column]; }
            set { _values[row, column] = value; }
        }

        public int Rows { get; private set; }
        public int Columns { get; private set; }
    }
}
