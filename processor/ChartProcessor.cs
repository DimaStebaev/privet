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
using System.Windows;
using Common;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;

namespace Processor
{
    public abstract class ChartProcessor : IProcessor
    {
        public abstract string title { get; }
        public abstract string name {get; }
        public virtual UIElement process(Function f)
        {
            if (f == null) return null;

            LineGraph functionGraph = new LineGraph();

            LinkedList<double> x = new LinkedList<double>(), y = new LinkedList<double>();

            for (double _x = f.minX; _x < f.maxX + f.step / 2; _x += f.step)
            {
                x.AddLast(_x);
                y.AddLast(f.getValue(_x));
            }

            var xDataSource = x.AsXDataSource();
            var yDataSource = y.AsYDataSource();

            CompositeDataSource compositeDataSource = xDataSource.Join(yDataSource);
            functionGraph.DataSource = compositeDataSource;

            return (UIElement)functionGraph;
        }

        public void initialize()
        {
            throw new System.NotImplementedException();
        }

        protected abstract Function processFunction(Function f);

        public IList<Parameter> getParametersList()
        {
            throw new System.NotImplementedException();
        }

        public IList<string> checkParametersList(IList<Object> parameters)
        {
            throw new System.NotImplementedException();
        }

        public void setup(IList<Object> parameters)
        {
            throw new System.NotImplementedException();
        }
    }
}

