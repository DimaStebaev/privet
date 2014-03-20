using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using NLog;

namespace Common
{
    /// <summary>
    /// Форма для ввода набора параметров
    /// </summary>
    public class Form : StackPanel
    {
        #region Public methods

        /// <summary>
        /// Констуктор, который подучает набор параметров
        /// </summary>
        /// <param name="parameters">Набор параметров</param>
        public Form(IList<Parameter> parameters)
        {
            foreach (Parameter parameter in parameters)
                lines.Add(Line.makeLine(parameter));

            this.Children.Clear();
            foreach (Line line in lines)
                this.Children.Add(line);
        }

        /// <summary>
        /// Возвращает список значений, которые ввёл пользователь
        /// </summary>
        /// <returns>Список значений, которые ввёл пользователь</returns>
        public IList<Object> getValues()
        {
            IList<Object> values = new List<object>();

            foreach (Line line in lines)
                values.Add(line.getValue());

            return values;
        }

        /// <summary>
        /// Проверяет, есть ли ошибки ввода
        /// </summary>
        /// <returns>Список ошибок ввода</returns>
        public List<string> getErrors()
        {
            List<string> errors = new List<string>();
            foreach (Line line in lines)
            {
                string error = line.getError();
                if (error != null)
                    errors.Add(error);
            }

            return errors;
        }

        #endregion

        #region Fields
        List<Line> lines = new List<Line>();
        #endregion

        #region Private types

        /// <summary>
        /// Строка в форме
        /// </summary>
        private abstract class Line : Grid
        {
            #region Public methods

            /// <summary>
            /// Конструктор, принимающий параметр, который будет вводиться
            /// </summary>
            /// <param name="parameter">Параметр, который будет вводиться</param>
            public Line(Parameter parameter)
            {
                this.parameter = parameter;
            }

            /// <summary>
            /// Проверяет, есть ли ошибки ввода
            /// </summary>
            /// <returns>string с описанием, если есть ошибка, null в противном случае</returns>
            public abstract string getError();

            /// <summary>
            /// Возвращает значение параметра, которое ввёл пользователь
            /// </summary>
            /// <returns>Значение параметра</returns>
            public abstract Object getValue();

            public static Line makeLine(Parameter parameter)
            {
                if (parameter is Parameter.DoubleParameter)
                    return new DoubleInputLine(parameter as Parameter.DoubleParameter);
                else if (parameter is Parameter.BooleanParameter)
                {
                    Parameter.BooleanParameter booleanParameter = parameter as Parameter.BooleanParameter;
                    return new BooleanInputLine(booleanParameter
                        , parameter.title
                        , booleanParameter.trueTitle
                        , booleanParameter.falseTitle
                        );
                }
                else
                {
                    logger.Error("Unknown parameter type");
                    throw new ArgumentException("Unknown parameter type");
                }
            }

            #endregion

            #region Fields

            protected Parameter parameter;
            protected static Logger logger = LogManager.GetCurrentClassLogger();

            #endregion
        }

        /// <summary>
        /// Строка, состоящая из заголовка и поля для ввода
        /// </summary>
        private class DoubleInputLine : Line
        {
            #region Public methods

            /// <summary>
            /// Конструктор, принимающий параметр, который будет вводиться
            /// </summary>
            /// <param name="parameter">Параметр, который будет вводиться</param>
            public DoubleInputLine(Parameter.DoubleParameter parameter)
                : base(parameter)
            {
                this.ColumnDefinitions.Add(new ColumnDefinition());

                caption = new TextBlock();
                caption.Text = parameter.title;
                caption.Margin = new Thickness(5);
                caption.VerticalAlignment = VerticalAlignment.Center;
                caption.HorizontalAlignment = HorizontalAlignment.Left;

                edit = new TextBox();
                edit.Text = parameter.defaultValue.ToString();
                edit.Width = 60;
                edit.Margin = new Thickness(5);
                edit.VerticalAlignment = VerticalAlignment.Center;
                edit.HorizontalAlignment = HorizontalAlignment.Right;
                Grid.SetColumn(edit, 1);
                edit.TextChanged += edit_TextChanged;

                this.Children.Clear();
                this.Children.Add(caption);
                this.Children.Add(edit);

            }

            /// <summary>
            /// Проверяет, есть ли ошибки ввода
            /// </summary>
            /// <returns>string с описанием, если есть ошибка, null в противном случае</returns>
            override public string getError()
            {
                if (parameter.validate(edit.Text)) return null;

                return "Параметр \"" + parameter.title + "\" задан неверно";
            }

            /// <summary>
            /// Возвращает значение параметра, которое ввёл пользователь
            /// </summary>
            /// <returns>Значение параметра</returns>
            override public Object getValue()
            {
                if (!parameter.validate(edit.Text))
                {
                    logger.Error("Can't parse " + parameter.title + "parameter");
                    throw new System.ArgumentException("Can't parse " + parameter.title + "parameter");
                }

                return parameter.parse(edit.Text);
            }

            #endregion

            #region Fields

            TextBlock caption;
            TextBox edit;

            #endregion

            #region Private Methods
            void edit_TextChanged(object sender, TextChangedEventArgs e)
            {
                if (parameter.validate(edit.Text))
                    edit.Background = new SolidColorBrush(Colors.White);
                else
                    edit.Background = new SolidColorBrush(Colors.Pink);
            }
            #endregion
        }

        private class BooleanInputLine : Line
        {
            #region Public fields
            public BooleanInputLine(Parameter.BooleanParameter parameter
                , string caption
                , string trueTitle
                , string falseTitle
                )
                : base(parameter)
            {

                trueRadioButton = new RadioButton();
                trueRadioButton.Content = trueTitle;
                trueRadioButton.Margin = new Thickness(5, 5, 5, 0);
                RadioButton falseRadioButton = new RadioButton();
                falseRadioButton.Content = falseTitle;
                falseRadioButton.Margin = new Thickness(5);

                if ((bool)parameter.defaultValue)
                    trueRadioButton.IsChecked = true;
                else
                    falseRadioButton.IsChecked = true;

                GroupBox groupBox = new GroupBox();
                groupBox.Header = caption;
                groupBox.Margin = new Thickness(5);

                StackPanel sp = new StackPanel();

                sp.Children.Add(trueRadioButton);
                sp.Children.Add(falseRadioButton);
                groupBox.Content = sp;

                this.Children.Add(groupBox);
            }
            /// <summary>
            /// Проверяет, есть ли ошибки ввода
            /// </summary>
            /// <returns>string с описанием, если есть ошибка, null в противном случае</returns>
            override public string getError()
            {
                return null;
            }

            /// <summary>
            /// Возвращает значение параметра, которое ввёл пользователь
            /// </summary>
            /// <returns>Значение параметра</returns>
            override public Object getValue()
            {
                return trueRadioButton.IsChecked;
            }

            #endregion

            #region Fields
            RadioButton trueRadioButton;
            #endregion
        }

        #endregion
    }
}
