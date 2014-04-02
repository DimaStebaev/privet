using System;
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
                return "Модуль для работы с файлами формата *.CSV";
            }
        }

        public string name
        {
            get
            {
                return "csvSerializer";
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

            StreamWriter fileStream = new StreamWriter(filename);

            string lineToWrite = String.Empty;

            for (double x = f.minX; x <= f.maxX + f.step/2; x += f.step)
            {
                double y = f.getValue(x);

                lineToWrite = replaceCommasByPoints(x.ToString()) 
                    + ","
                    + replaceCommasByPoints(y.ToString());

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
            string format = Path.GetExtension(filename).ToLower();
            //Check file existence
            if (File.Exists(filename) &&
                format == ".csv")
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

                    for (int i = 0; i < values.Length; i++)
                        values[i] = modifyDecimalSeparator(values[i]);

                    //Check only strings with two double values or with double values and the "" at the end
                    if (
                        (
                            (values.Length == 2)
                            ||
                            (values.Length > 2 && values[2] == "")
                        )
                        &&
                        Double.TryParse(values[0], out currentX)
                        &&
                        Double.TryParse(values[1], out currentY)
                    )
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

                if (mXList.Count < 2) throw new InvalidDataException("File corrupted.");

                //Find maximum and minimum x values
                var minX = mXList.Min();
                var maxX = mXList.Max();
                //Calculate step
                var step = (maxX - minX) / (mXList.Count - 1);

                functionToReadIn.setup(minX, maxX, step);

                for (int i = 0; i < mXList.Count; i++)
                    functionToReadIn.setValue(mXList[i], mYList[i]);
                
                return functionToReadIn;                
            }
            else
            {
                throw new FileNotFoundException("Filename is incorrect.");
            }
        }       

        /// <summary>
        /// Replace commas by points
        /// </summary>
        private string replaceCommasByPoints(string str)
        {
            StringBuilder sb = new StringBuilder(str);

            sb.Replace(',', '.');

            return sb.ToString();
        }

        /// <summary>
        /// Replace commas and points by system locale decimal separator
        /// </summary>       
        private string modifyDecimalSeparator(string str)
        {
            StringBuilder sb = new StringBuilder(str);

            char sep = Convert.ToChar(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            sb.Replace('.', sep);
            sb.Replace(',', sep);

            return sb.ToString();
        }

        public virtual void initialize()
        {
            
        }

        public IList<string> checkParametersList(IList<Object> parameters)
        {
            return new List<string>();
        }

        public void setup(IList<Object> parameters)
        {
            
        }

        public IList<Parameter> getParametersList()
        {
            return new List<Parameter>();
        }
    }
}
