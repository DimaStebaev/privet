//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool
//     Changes to this file will be lost if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Generator;
using Common;

namespace HarmonicFunctionGenerator
{   
    /// <summary>
    /// Генератор гармонической функции
    /// </summary>
    public class HarmonicFunctionGenerator : IGenerator
    {
        // Параметры функций синусоид
        private double a;
        private double b;
        private double c;

        public string title
        {
            get
            {
                return "Гармоническая функция";
            }
        }
        public string name
        {
            get
            {
                return "HarmonicFunction";
            }
        }

        /// <summary>
        /// Генерирует гармоническую функцию
        /// </summary>
        /// <param name="minX">Минимальное значение аргумента</param>
        /// <param name="maxX">Максимальное значение аргумента</param>
        /// <param name="step">Шаг аргумента</param>
        /// <returns>Гармоническую функцию</returns>
        public virtual Function Generate(double minX, double maxX, double step)
	    {
            Function result = new Function();
            result.setup(minX, maxX, step);

            for (double x = minX; x < maxX + step / 2; x += step)
            {
                double y = Math.Sin(2 * Math.PI * a * x);
                y += Math.Sin(2 * Math.PI * b * x);
                y += Math.Sin(2 * Math.PI * c * x);
                result.setValue(x, y);
            }

            return result;
	    }

        /// <summary>
        /// Получение параметров генератора
        /// </summary>
        /// <returns>Список параметро</returns>
	    public virtual IList<Parameter> getParametersList()
	    {
            List<Parameter> p = new List<Parameter>();
            Parameter A = Parameter.Double("Частота 1й синусоиды", 3);
            Parameter B = Parameter.Double("Частота 2й синусоиды", 2);
            Parameter C = Parameter.Double("Частота 3й синусоиды", 1);
            p.Add(A);
            p.Add(B);
            p.Add(C);
            return p;
	    }

        /// <summary>
        /// Инициализация параметров генератора
        /// </summary>
        /// <param name="parameters">Список параметров</param>
	    public virtual void setup(IList<Object> parameters)
	    {
            if (checkParametersList(parameters).Count>0)   // Валидация параметров
                throw new ArgumentException("Not available argument");

            a = (double)parameters[0];
            b = (double)parameters[1];
            c = (double)parameters[2];
	    }

        /// <summary>
        /// Инициализация по умолчанию
        /// </summary>
	    public virtual void initialize()
	    {
	    }

        /// <summary>
        /// Валидация параметров
        /// </summary>
        /// <param name="parameters">Список параметров</param>
        /// <returns>Успех / ошибка</returns>
        public IList<string> checkParametersList(IList<Object> parameters)
        {
            //TODO: нужно возвращать список ошибок
            throw new System.NotImplementedException();

            /*
            // Если количество параметров 3 - верно
            if(parameters.Count==3)
            {
                // Если все из параметров double - верно
                if (parameters[0] is double &&
                    parameters[1] is double &&
                    parameters[2] is double)
                {
                    double _a = (double)parameters[0];
                    double _b = (double)parameters[1];
                    double _c = (double)parameters[2];
                    // Если выполняется условие a>b>c - верно
                    if (_a > _b && _b > _c)
                        return true;
                }
            }
            return false;
             */
        }
    }
}


