using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Generator;
using Common;

namespace sinGenerator
{
    public class SinGenerator: IGenerator
    {
        double a = 1;
        public Function Generate(double minX, double maxX, double step)
        {
            Function result = new Function();
            result.setup(minX, maxX, step);

            for (double x = minX; x < maxX + step / 2; x += step)
                result.setValue(x, Math.Sin(a*x));

            return result;
        }
        public string title
        {
            get
            {
                return "Синус";
            }
        }
        public string name
        {
            get
            {
                return "SinGenerator";
            }
        }
        public virtual IList<Parameter> getParametersList()
        {
            List<Parameter> p = new List<Parameter>();
            Parameter A = Parameter.Double("Частота", 1);
            p.Add(A);
            return p;
        }

        public virtual void setup(IList<Object> parameters)
        {
            if (checkParametersList(parameters).Count > 0)
                throw new Exception("Bad parameter");

            a = (double)parameters[0];
        }

        public virtual void initialize()
        {
            
        }

        public IList<string> checkParametersList(IList<Object> parameters)
        {
            List<string> errors = new List<string>();
            if (parameters.Count != 1) errors.Add("Неправильное количество параметров");

            if (parameters.Count < 1) return errors;

            if (!(parameters[0] is double))
            {
                errors.Add("Частота должна быть вещественным числом");
                return errors;
            }

            double a = (double)parameters[0];
            if (a < 1e-6) errors.Add("Частота должна быть строго больше нуля");
            return errors;
        }
    }
}
