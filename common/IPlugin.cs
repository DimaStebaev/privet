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

namespace Common
{
    public interface IPlugin 
    {
	    string title { get; }

        string name { get; }   
     
	    void initialize();

	    IList<Parameter> getParametersList();

	    void setup(IList<Object> parameters);

	    bool checkParametersList(IList<Object> parameters);

    }
}