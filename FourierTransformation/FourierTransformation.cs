﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool
//     Changes to this file will be lost if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Processor;
using System.Numerics;

namespace FourierTransformation
{
    public class FourierTransformation : ChartProcessor
    {
        public override string title
        {
            get 
            {
                return "Частотные характеристики"; 
            }
        }
        public override string name
        {
            get
            {
                return "FourierTransformation";    
            }
        }

        protected override Common.Function processFunction(Common.Function f)
        {
            Complex[] input = new Complex[f.Length];
            Complex[] output;

            for (int i = 0; i < f.Length; i++)
            {
                input[i] = new Complex(f[i], 0);
            }

            output = DFT(input, Direction.Forward);

            Common.Function resultFunction = new Common.Function();
            resultFunction.setup(f.minX, f.maxX, f.step);

            for (int i = 0; i < resultFunction.Length; i++)
            {
                resultFunction[i] = output[i].Magnitude;
            }

            return resultFunction;
        }

        public enum Direction
        {
            /// <summary>
            /// Forward direction of Fourier transformation.
            /// </summary>
            Forward = 1,

            /// <summary>
            /// Backward direction of Fourier transformation.
            /// </summary>
            Backward = -1
        };

        private Complex[] DFT(Complex[] data, Direction direction)
        {
            int n = data.Length;
            double arg, cos, sin;
            Complex[] dst = new Complex[n];

            // for each destination element
            for (int i = 0; i < n; i++)
            {
                dst[i] = Complex.Zero;

                arg = -(int)direction * 2.0 * System.Math.PI * (double)i / (double)n;

                // sum source elements
                for (int j = 0; j < n; j++)
                {
                    cos = System.Math.Cos(j * arg);
                    sin = System.Math.Sin(j * arg);

                    dst[i] += new Complex((data[j].Real * cos - data[j].Imaginary * sin),
                                          (data[j].Real * sin + data[j].Imaginary * cos));
                }
            }

            // copy elements
            if (direction == Direction.Forward)
            {
                // devide also for forward transform
                for (int i = 0; i < n; i++)
                {
                    dst[i] = dst[i] / n;
                }
            }
            return dst;
        }

    }
}

