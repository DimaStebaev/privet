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

namespace Processor
{
    public class ProcessingManager
    {       

        public virtual UIElement process(IProcessor processor, Function f)
        {
            return processor.process(f);
        }

    }
}
