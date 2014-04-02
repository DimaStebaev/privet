using System;
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

            if (generatorSelector.Children[0] is ComboBox)
            {
                ComboBox com = generatorSelector.Children[0] as ComboBox;
                com.Loaded += UpdateWindow;
                com.SelectionChanged += UpdateWindow;
            }

            noiseForm = new Form(noise);
            noiseFormControl.Content = noiseForm;

            noiseSelector = new PluginSelector(typeof(INoise));
            noiseSelectorControl.Content = noiseSelector;

            if (noiseSelector.Children[0] is ComboBox)
            {
                ComboBox com = generatorSelector.Children[0] as ComboBox;
                com.Loaded += UpdateWindow;
                com.SelectionChanged += UpdateWindow;
            }
        }

        /// <summary>
        /// Переопределение минимального размера окна
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e"></param>
        private void UpdateWindow(object sender, EventArgs e)
        {
            paramsContent.UpdateLayout();
            this.MinHeight = paramsContent.ActualHeight + 70;
            this.MinWidth = paramsContent.ActualWidth + 500;
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
            else
            {
                functionGraph.DataSource = new CompositeDataSource();
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
            else
            {
                noisedFunctionGraph.DataSource = new CompositeDataSource();
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
                try
                {
                    foreach (IPlugin plugin in plugins)
                    {
                        ISerializer s = plugin as ISerializer;
                        if (s.extension.Equals(ext))
                        {
                            generatedFunction = s.deserialize(filename);
                            EtalonFunction = null;
                            drawFunction(EtalonFunction, generatedFunction);
                            return;
                        }
                    }
                }
                catch(Exception ex)
                {
                    logger.Error("Loading function error: " + ex.ToString());
                    MessageBox.Show(ex.Message.ToString());
                    return;
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

        #endregion

        

    }    
}
