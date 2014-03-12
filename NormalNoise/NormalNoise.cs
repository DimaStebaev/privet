using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Generator;


namespace NormalNoise
{
    /// <summary>
    /// Generates normal distributed random values with mean = 0.0 and deviation = 1.0.
    /// Use getDeviation() method to get next random value.
    /// </summary>
    public class NormalNoise : INoise
    {
        #region Private Members

        private double          mean                    = 0.0;
        private double          deviation               = 1.0;

        Random                  uniformRandomGenerator  = new Random();
        private bool            isReady                 = false;
        private double          second                  = 0.0;
        
        
        #endregion

        #region Parameters
        public string title
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }
        public string name
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Generates normal distributed random value
        /// </summary>
        /// <returns>Returns normal distributed random value</returns>
	    public virtual double getDeviation()
	    {
            if (isReady)
            {
                isReady = false;
                return second * deviation + mean;
            }
            else
            {
                double u, v, s;
                do
                {
                    u = 2.0 * (double)uniformRandomGenerator.Next(-100, 100) / 100 - 1.0;
                    v = 2.0 * (double)uniformRandomGenerator.Next(-100, 100) / 100 - 1.0;
                    s = u * u + v * v;
                } while (s > 1.0 || s == 0.0);

                double r = Math.Sqrt(-2.0 * Math.Log(s) / s);
                second = r * u;
                isReady = true;
                return r * v * deviation + mean;
            }
	    }

	    public virtual IList<Parameter> getParametersList()
	    {
		    throw new System.NotImplementedException();
	    }

	    public virtual void setup(IList<Object> parameters)
	    {
		    throw new System.NotImplementedException();
	    }

	    public virtual void initialize()
	    {
		    throw new System.NotImplementedException();
	    }

        public bool checkParametersList(IList<Object> parameters)
        {
            throw new System.NotImplementedException();
        }
        #endregion

    }
}
