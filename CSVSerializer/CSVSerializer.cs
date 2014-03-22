﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Common;

namespace CSVSerializer
{
    public class CSVSerializer : ISerializer
    {
        #region Private Members
        private List<double> mXList;
        private List<double> mYList;
        #endregion

        #region properties

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

        public string extension
        {
            get
            {
                return "csv";
            }
        }

        #endregion

        /// <summary>
        /// Writes Function f to output CSV file
        /// </summary>
        /// <param name="f">Function to write</param>
        /// <param name="filename">Output file path</param>
        public virtual void serialize(Function f, string filename)
        {
            //Check existence 
            if (!File.Exists(filename))
            {
                File.Create(filename);
            }

            StreamWriter fileStream = new StreamWriter(filename);

            string lineToWrite = String.Empty;

            for (double x = f.minX; x <= f.maxX; x += f.step)
            {
                double y = f.getValue(x);

                lineToWrite = x.ToString() + "," + y.ToString();
                fileStream.WriteLine(lineToWrite);
            }

            fileStream.Close();
        }

        /// <summary>
        /// Reads Function from CSV file. Each line in this file must have format "x,y".
        /// </summary>
        /// <param name="filename">Filename</param>
        /// <returns>Function</returns>
        public virtual Function deserialize(string filename)
        {
            //Check file existence
            if (File.Exists(filename))
            {
                Function functionToReadIn = new Function();
                StreamReader fileStream = new StreamReader(filename);
                string lineToRead = String.Empty;
                string[] values = null;
                char[] separators = { ',' };
                double currentX = 0.0;
                double currentY = 0.0;

                mXList = new List<double>();
                mYList = new List<double>();


                while (!fileStream.EndOfStream)
                {
                    lineToRead = fileStream.ReadLine();
                    values = lineToRead.Split(separators);
                    //Check only strings with two double values or with double values and the "" at the end
                    if (((values.Length == 2) || (values.Length > 2 && values[2] == "")) && Double.TryParse(values[0], out currentX) && Double.TryParse(values[1], out currentY))
                    {
                        mXList.Add(currentX);
                        mYList.Add(currentY);
                    }
                    else
                    {
                        //If line couldn't be parsed -> throw exception
                        fileStream.Close();
                        throw new InvalidDataException("File corrupted.");
                    }
                }
                fileStream.Close();

                //Find maximum and minimum x values
                var minX = mXList.Min();
                var maxX = mXList.Max();
                //Calculate step
                var step = (maxX - minX) / (mXList.Count - 1);

                if (checkParsedData(minX, maxX, step))
                {
                    //And setup output function
                    functionToReadIn.setup(minX, maxX, step);
                    //Populate all values
                    for (int i = 0; i < mXList.Count; i++)
                    {
                        functionToReadIn.setValue(mXList[i], mXList[i]);
                    }
                    return functionToReadIn;
                }
                else
                {
                    throw new InvalidDataException("File corrupted.");
                }
            }
            else
            {
                throw new FileNotFoundException("Filename is incorrect.");
            }
        }

        /// <summary>
        /// Checks the data that was parsed through deserialize method
        /// </summary>
        /// <returns>True, if data is ok</returns>
        private bool checkParsedData(double minX, double maxX, double step)
        {
            int i = 0;

            for (double x = minX; x <= maxX; x += step)
            {
                //Validate X, check step should be the same
                if (x != mXList[i])
                {
                    return false;
                }
                i++;
            }

            return true;
        }

        public virtual void initialize()
        {
            
        }

        public IList<string> checkParametersList(IList<Object> parameters)
        {
            throw new System.NotImplementedException();
        }

        public void setup(IList<Object> parameters)
        {
            throw new System.NotImplementedException();
        }

        public IList<Parameter> getParametersList()
        {
            throw new System.NotImplementedException();
        }
    }
}