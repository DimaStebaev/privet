using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NLog;

namespace Common
{    
    /// <summary>
    /// Контейнер для функции вида f: R -> R, заданной как набор пар (x, y)
    /// </summary>
    public class Function
    {
        static Logger logger = LogManager.GetCurrentClassLogger();
        static readonly double EPS = 1e-7;

        double[] values = null;

        /// <summary>
        /// Минимальное значение аргумента, на котором задана функция.
        /// </summary>
        public virtual double minX
        {
            get;
            private set;
        }

        /// <summary>
        /// Максимальное значение аргумента, на котором задана функция.
        /// </summary>
        public virtual double maxX
        {
            get;
            private set;
        }

        /// <summary>
        /// Растояние между соседними значениями x
        /// </summary>
        public virtual double step
        {
            get;
            private set;
        }

        /// <summary>
        /// Количество значений функции
        /// </summary>
        public virtual int length
        {
            get { return values.Length; }
        }

        /// <summary>
        /// Вичисляет значение функции в точке x
        /// </summary>
        /// <param name="x">Значение аргумента функции</param>
        /// <returns>Значение функции</returns>
        public virtual double getValue(double x)
        {
            if(values == null || values.Length == 0)
            {
                logger.Fatal("Function values is not initialized");
                throw new NullReferenceException("Function values is not initialized");
            }

            if(minX > maxX)
            {
                logger.Fatal("left edge of function greater then right");
                throw new Exception("left edge of function greater then right");
            }

            if (Math.Abs(minX - maxX) < EPS) return values[0];                        
            
            int leftIndex = getIndexOfRightestLeftX(x);
            int rightIndex = getIndexOfLeftestRightX(x);

            if (leftIndex < 0) leftIndex = 0;
            if (rightIndex < 0) rightIndex = 0;
            if (leftIndex >= values.Length) leftIndex = values.Length - 1;
            if (rightIndex >= values.Length) rightIndex = values.Length - 1;

            if (leftIndex == rightIndex) return values[leftIndex];

            double leftX = minX + leftIndex * step;
            double rightX = minX + rightIndex * step;
            if (Math.Abs(rightX - leftX) < EPS) return values[leftIndex];

            double k = (x - leftX) / (rightX - leftX);            

            return values[leftIndex] + (values[rightIndex] - values[leftIndex]) * k;
        }

        /// <summary>
        /// Индексатор, аналог перегрузки оператора []
        /// </summary>
        /// <param name="index">Значение индекса </param>
        /// <returns>Значение функции по индексу</returns>
        public double this[int index]
        {
            get
            {
                return values[index];
            }
            set
            {
                values[index] = value;
            }
        }

        /// <summary>
        /// Устанавливает значение функции в точке
        /// </summary>
        /// <param name="x">Значение аргумента функции</param>
        /// <param name="value">Значение функции в точке</param>
        public virtual void setValue(double x, double value)
        {
            if(x < minX + EPS)
            {
                values[0] = value;
                return;
            }
            if(x > maxX - EPS)
            {
                values[values.Length - 1] = value;
                return;
            }

            int leftIndex = getIndexOfRightestLeftX(x);
            int rightIndex = getIndexOfLeftestRightX(x);

            if(leftIndex == rightIndex)
            {
                values[leftIndex] = value;
                return;
            }

            double leftX = minX + leftIndex * step;
            double rightX = minX + rightIndex * step;

            if(Math.Abs(leftX - x) < Math.Abs(rightX - x))
            {
                values[leftIndex] = value;
            }
            else
            {
                values[rightIndex] = value;
            }
        }

        /// <summary>
        /// Инициализирует функцию
        /// </summary>
        /// <param name="minX">Минимальное значение аргумента</param>
        /// <param name="maxX">Максимальное значение аргумента</param>
        /// <param name="step">Растояние между соседними значениями X</param>
        public virtual void setup(double minX, double maxX, double step)
        {
            this.minX = Math.Min(minX, maxX);
            this.maxX = Math.Max(minX, maxX);
            this.step = step;

            int n = (int)Math.Floor( (this.maxX - this.minX) / step + EPS) + 1;

            values = new double[n];
            for (int i = 0; i < n; i++)
                values[i] = 0;
        }

        /// <summary>
        /// Возвращает индекс самой правой точки, которая находится слева от X
        /// </summary>
        /// <param name="x">Значение аргумента</param>
        /// <returns>Индекс</returns>
        private int getIndexOfRightestLeftX(double x)
        {
            if (values == null || values.Length == 0)
            {
                logger.Fatal("Function values is not initialized");
                throw new NullReferenceException("Function values is not initialized");
            }

            if (minX > maxX)
            {
                logger.Fatal("left edge of function greater then right");
                throw new Exception("left edge of function greater then right");
            }

            if (x < minX + EPS) return 0;
            if (x > maxX - EPS) return values.Length - 1;

            if (Math.Abs(minX - maxX) < EPS) return 0;

            double k = (x - minX) / (maxX - minX);
            return (int)Math.Floor((values.Length - 1) * k);            
        }

        /// <summary>
        /// Возвращает индекс самой левой точки, которая находится справа от X
        /// </summary>
        /// <param name="x">Значение аргумента</param>
        /// <returns>Индекс</returns>
        private int getIndexOfLeftestRightX(double x)
        {
            if (values == null || values.Length == 0)
            {
                logger.Fatal("Function values is not initialized");
                throw new NullReferenceException("Function values is not initialized");
            }

            if (minX > maxX)
            {
                logger.Fatal("left edge of function greater then right");
                throw new Exception("left edge of function greater then right");
            }

            if (x < minX + EPS) return 0;
            if (x > maxX - EPS) return values.Length - 1;

            if (Math.Abs(minX - maxX) < EPS) return 0;

            double k = (x - minX) / (maxX - minX);            
            return (int)Math.Ceiling((values.Length - 1) * k);
        }
    }
}

