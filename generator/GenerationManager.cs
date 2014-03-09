using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NLog;
using Common;

namespace Generator
{
    /// <summary>
    /// Генерирует функцию с учётом погрешности
    /// </summary>
    public class GenerationManager
    {
        static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Генерирует функцию с учётом погрешности
        /// </summary>
        /// <param name="minX">Минимальное значение X</param>
        /// <param name="maxX">Максимальное значение X</param>
        /// <param name="step">Разница между двумя соседними значениями X</param>
        /// <param name="generator">Генератор</param>
        /// <param name="generatorParameters">Список параметров генератора</param>
        /// <param name="noise">Погрешность</param>
        /// <param name="noiseParameters">Список параметров погрешности</param>
        /// <returns>Функцию заданного вида с заданной погрешностью</returns>
        public virtual Function generate(double minX, double maxX, double step
            , IGenerator generator, IList<Object> generatorParameters
            , INoise noise, IList<Object> noiseParameters)
        {
            if(generator == null)
            {
                logger.Error("generator is null");
                throw new ArgumentNullException("generator is null");
            }

            if(!generator.checkParametersList(generatorParameters))
            {
                logger.Error("Wrong generator arguments");
                throw new ArgumentException("Wrong generator arguments");
            }

            generator.setup(generatorParameters);

            Function f = generator.Generate(minX, maxX, step);

            if(noise != null)
            {
                if(!noise.checkParametersList(noiseParameters))
                {
                    logger.Error("Wrong noise arguments");
                    throw new ArgumentException("Wrong noise arguments");
                }

                noise.setup(noiseParameters);                
                
                Noiser noiser = new Noiser();
                f = noiser.addNoise(f, noise);                 
            }

            return f;
        }
    }
}
