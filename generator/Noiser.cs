using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace Generator
{
    /// <summary>
    /// Добавляет погрешность к функции
    /// </summary>
    public class Noiser
    {
        /// <summary>
        /// Добавляет погрешность которая пропорциональна амплитуде функции
        /// </summary>
        /// <param name="f">Функция, к которой будет добавлна погрешность</param>
        /// <param name="noise">Тип погрешности</param>
        /// <param name="k">Чем больше K, тем больше погрешность</param>
        /// <returns>Функцию с добавленной погрешностью</returns>
        public virtual Function addNoise(Function f, INoise noise, double k)
        {
            Function result = new Function();
            result.setup(f.minX, f.maxX, f.step);
            for (int i = 0; i < f.Length; i++)
                result[i] = f[i] + k * f[i] * noise.getDeviation();
            return result;
        }

    }
}
