﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Win32;

using NLog;
using Common;

namespace Generator
{
    /// <summary>
    /// Главная форма программы-генератора
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Public methods

        /// <summary>
        /// Конструктор
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();            

            refreshControlPanel();

            chart.LegendVisible = false;
        }

        #endregion

        /// <summary>
        /// Перестраивает панель с формами
        /// </summary>
        private void refreshControlPanel()
        {
            List<Parameter> general = new List<Parameter>();
            general.Add(Parameter.Double("Левая граница", 0));
            general.Add(Parameter.Double("Правая граница", 10));
            general.Add(Parameter.Double("Шаг", 0.1));            

            List<Parameter> noise = new List<Parameter>();
            noise.Add(Parameter.Double("Коэффицент", 0.3));
            noise.Add(new Parameter.BooleanParameter("Тип погрешности", "Относительная", "Абсолютная", true));

            generalForm = new Form(general);
            generalFormControl.Content = generalForm;

            generatorSelector = new PluginSelector(typeof(IGenerator));
            generatorSelectorControl.Content = generatorSelector;

            noiseForm = new Form(noise);
            noiseFormControl.Content = noiseForm;

            noiseSelector = new PluginSelector(typeof(INoise));
            noiseSelectorControl.Content = noiseSelector;            
        }

        /// <summary>
        /// Обработчик кнопки "Генерировать"
        /// </summary>        
        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            logger.Info("Start generating");

            if (!validateInputs()) return;

            GenerationManager gm = new GenerationManager();

            IGenerator generator = generatorSelector.getSelectedPlugin() as IGenerator;
            INoise noise = noiseSelector.getSelectedPlugin() as INoise;

            IList<Object> param = generalForm.getValues();
            double minX = (double)param[0]
                , maxX = (double)param[1]
                , step = (double)param[2];

            param = noiseForm.getValues();
            double k = (double)param[0];
            bool isRelative = (bool)param[1];

            try
            {
                EtalonFunction  = gm.generate(minX, maxX, step
                                        , generator, generatorSelector.getPluginParametersValues()
                                        , null, null
                                        , k
                                        , false
                                        );
                generatedFunction = gm.generate(minX, maxX, step
                                        , generator, generatorSelector.getPluginParametersValues()
                                        , noise, noiseSelector.getPluginParametersValues()
                                        , k
                                        , isRelative
                                        );

                drawFunction(EtalonFunction, generatedFunction);
            }
            catch (Exception ex)
            {
                logger.Error("Generation error: " + ex.ToString());
                MessageBox.Show("Ошибка генерирования");
            }
        }        

        #region Fields

        Form generalForm = null;
        Form noiseForm = null;
        PluginSelector generatorSelector = null;
        PluginSelector noiseSelector = null;
        static Logger logger = LogManager.GetCurrentClassLogger();
        PluginManager pluginManager = PluginManager.getPluginManager();
        Function _generatedFunction = null;        

        Function generatedFunction
        {
            get
            {
                return _generatedFunction;
            }
            set
            {
                _generatedFunction = value;
                saveAsMenuItem.IsEnabled = (_generatedFunction != null);
            }
        }

        Function EtalonFunction
        {
            get ; set;
        }

        #endregion  

        /// <summary>
        /// Проверяет, что все поля содержат допустимые значения
        /// </summary>
        private bool validateInputs()
        {
            List<string> errors = new List<string>();
            errors.AddRange(generalForm.getErrors());
            errors.AddRange(noiseForm.getErrors());
            errors.AddRange(generatorSelector.getErrors());
            errors.AddRange(noiseSelector.getErrors());

            try
            {
                IList<Object> param = generalForm.getValues();
                double step = (double)param[2];

                if (step < 0) errors.Add("Шаг должен быть положительным");
                else if (step < GlobalConstants.Precision) errors.Add("Шаг слишком маленький");
            }
            catch (ArgumentException e)
            {
                //пользователь неверно ввёл step
            }

            try
            {
                IList<Object> param = generalForm.getValues();
                double minX = (double)param[0]
                    , maxX = (double)param[1]
                    , step = (double)param[2];

                if (maxX - minX < step + GlobalConstants.Precision) errors.Add("Шаг слижком большой");
            }catch(ArgumentException e)
            {
                //пользователь неверно ввёл minX, maxX или step
            }

            try
            {
                IList<Object> param = noiseForm.getValues();
                double k = (double)param[0];
                if (k < 0 || k > 1) errors.Add("Коэффицент погрешности должен находиться в пределах от 0 до 1");
            }catch(ArgumentException e)
            {
                //пользователь неверно ввёл коэффицент K
            }

            if (errors.Count > 0)
            {
                string errorMessage = "";
                foreach(string line in errors)
                {
                    if(errorMessage != "") errorMessage += Environment.NewLine;
                    errorMessage += line;
                }
                MessageBox.Show(errorMessage);
                return false;
            }
            else
                return true;
        }        

        /// <summary>
        /// Отображает функцию f на графике
        /// </summary>        
        private void drawFunction(Function f, Function noisedF)
        { 
            
            if(f!=null)
            {
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
            }

            if (noisedF!=null)
            {
                LinkedList<double> x = new LinkedList<double>(), y = new LinkedList<double>();

                for (double _x = noisedF.minX; _x < noisedF.maxX + noisedF.step / 2; _x += noisedF.step)
                {
                    x.AddLast(_x);
                    y.AddLast(noisedF.getValue(_x));
                }

                var xDataSource = x.AsXDataSource();
                var yDataSource = y.AsYDataSource();

                CompositeDataSource compositeDataSource = xDataSource.Join(yDataSource);

                noisedFunctionGraph.DataSource = compositeDataSource;                
            }           

            chart.LegendVisible = false;
            chart.FitToView();
        }        

        
        /// <summary>
        /// Обработчик нажатия на пункт меню "Выход"
        /// </summary>        
        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Обработчик нажатия на пункт меню "Открыть..."
        /// </summary>  
        private void MenuItemOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            IList<IPlugin> plugins = pluginManager.getBeans(typeof(ISerializer));

            if(plugins.Count<1)
            {
                MessageBox.Show("Невозвожно загрузить функцию, так как отсутствуют соответствующие плагины");
                logger.Error("Serializers are missing");
                return;
            }

            dialog.Filter = "Файл с функцией|";
            bool first = true;
            foreach(IPlugin plugin in plugins)
            {
                ISerializer s = plugin as ISerializer;

                if(first)
                    first = false;
                else
                    dialog.Filter += ";";

                dialog.Filter += "*." + s.extension;
            }

            Nullable<bool> result = dialog.ShowDialog();
            if(result == true)
            {
                String filename = dialog.FileName;
                string[] splitted = filename.Split('.');
                string ext = splitted[splitted.Length - 1];

                foreach(IPlugin plugin in plugins)
                {
                    ISerializer s = plugin as ISerializer;
                    if(s.extension.Equals(ext))
                    {
                        generatedFunction = s.deserialize(filename);
                        EtalonFunction = null;
                        drawFunction(EtalonFunction, generatedFunction);
                        return;
                    }
                }

                logger.Error("Loading function error");
                MessageBox.Show("Ошибка во время загрузки функции");
            }
        }

        /// <summary>
        /// Обработчик нажатия на пункт меню "Сохранить как..."
        /// </summary>  
        private void MenuItemSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();

            IList<IPlugin> plugins = pluginManager.getBeans(typeof(ISerializer));

            if (plugins.Count < 1)
            {
                MessageBox.Show("Невозвожно сохранить функцию, так как отсутствуют соответствующие плагины");
                logger.Error("Serializers are missing");
                return;
            }

            dialog.Filter = "Файл с функцией|";
            bool first = true;
            foreach (IPlugin plugin in plugins)
            {
                ISerializer s = plugin as ISerializer;

                if (first)
                    first = false;
                else
                    dialog.Filter += ";";

                dialog.Filter += "*." + s.extension;
            }

            Nullable<bool> result = dialog.ShowDialog();
            if (result == true)
            {
                String filename = dialog.FileName;
                string[] splitted = filename.Split('.');
                string ext = splitted[splitted.Length - 1];

                foreach (IPlugin plugin in plugins)
                {
                    ISerializer s = plugin as ISerializer;
                    if (s.extension.Equals(ext))
                    {
                        s.serialize(generatedFunction, filename);
                        return;
                    }
                }

                logger.Error("Saving function error");
                MessageBox.Show("Ошибка при сохранении функции");
            }
        }

        /// <summary>
        /// Обработчик нажатия на пункт меню "Обновить плагины"
        /// </summary>  
        private void MenuItemRefreshPlugins_Click(object sender, RoutedEventArgs e)
        {
            pluginManager.refresh();
            refreshControlPanel();
        }

        #region Private types

        /// <summary>
        /// Форма для ввода набора параметров
        /// </summary>
        private class Form : StackPanel
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
                foreach(Line line in lines)
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
                foreach(Line line in lines)
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

                #endregion                
            }

            /// <summary>
            /// Строка, состоящая из заголовка и поля для ввода
            /// </summary>
            private class DoubleInputLine: Line
            {
                #region Public methods

                /// <summary>
                /// Конструктор, принимающий параметр, который будет вводиться
                /// </summary>
                /// <param name="parameter">Параметр, который будет вводиться</param>
                public DoubleInputLine(Parameter.DoubleParameter parameter):base(parameter)
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
                    if(!parameter.validate(edit.Text))
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

            private class BooleanInputLine: Line
            {
                #region Public fields
                public BooleanInputLine(Parameter.BooleanParameter parameter
                    , string caption
                    , string trueTitle
                    , string falseTitle
                    ):base(parameter)
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

        private class PluginSelector : StackPanel
        {
            #region Public methods

            /// <summary>
            /// Конструктор
            /// </summary>
            /// <param name="pluginType">Тип плагинов, которые будут находиться в выпадающем списке</param>
            public PluginSelector(Type pluginType)
            {
                this.pluginType = pluginType;

                plugins = pluginManager.getBeans(pluginType);

                comboBox = new ComboBox();
                comboBox.Margin = new Thickness(5);
                foreach(IPlugin plugin in plugins)
                {
                    ComboBoxItem item = new ComboBoxItem();
                    item.Content = plugin.title;
                    item.Tag = plugin;

                    comboBox.Items.Add(item);
                }

                comboBox.SelectionChanged += comboBox_SelectionChanged;

                if (comboBox.Items.Count > 0)
                    comboBox.SelectedItem = comboBox.Items[0];

                this.Children.Clear();
                this.Children.Add(comboBox);
                this.Children.Add(parametersForm);
            }

            /// <summary>
            /// Проверяет, есть ли ошибки ввода в форме для ввода параметров
            /// </summary>
            /// <returns>Список ошибок ввода</returns>
            public IList<string> getErrors()
            {
                if (form == null || comboBox.SelectedItem == null) return new List<string>();

                List<string> errors = form.getErrors();

                if (errors.Count > 0) return errors;

                IPlugin plugin = (comboBox.SelectedItem as ComboBoxItem).Tag as IPlugin;
                errors.AddRange(plugin.checkParametersList(form.getValues()));

                return errors;

            }

            /// <summary>
            /// Возвращает список значений параметров, которые ввёл пользователь
            /// </summary>
            /// <returns>Список значений, которые ввёл пользователь</returns>
            public IList<Object> getValues()
            {
                if (form == null) return new List<Object>();

                return form.getValues();
            }

            /// <summary>
            /// Возвращает выбранный генератор
            /// </summary>
            /// <returns>Вебранный генератор</returns>
            public IPlugin getSelectedPlugin()
            {                
                if(comboBox.SelectedItem == null) return null;
                return (comboBox.SelectedItem as ComboBoxItem).Tag as IPlugin;
            }

            /// <summary>
            /// Возвращает значения параметров плагина
            /// </summary>
            /// <returns>Значения параметров плагина</returns>
            public IList<Object> getPluginParametersValues()
            {
                if (form == null) return new List<Object>();
                return form.getValues();
            }
            
            #endregion

            #region Fields

            Type pluginType;
            IList<IPlugin> plugins;
            ComboBox comboBox;
            ContentControl parametersForm = new ContentControl();
            Form form = null;
            PluginManager pluginManager = PluginManager.getPluginManager();

            #endregion

            #region Private methods

            /// <summary>
            /// Обработчик изменения выбраного плагина
            /// </summary>            
            void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if(comboBox.SelectedItem == null) return;

                IPlugin selectedPlugin = (comboBox.SelectedItem as ComboBoxItem).Tag as IPlugin;

                form = new Form(selectedPlugin.getParametersList());

                parametersForm.Content = form;
            }
            #endregion
        }

        #endregion

        

    }    
}
