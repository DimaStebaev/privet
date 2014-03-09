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

        /// <summary>
        /// Конструктор
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();       

            generatorsComboBox.Items.Clear();            
            noisesComboBox.Items.Clear();
            generatorParameters.Children.Clear();
            noiseParameters.Children.Clear();

            generatorsComboBox.SelectionChanged += generatorsComboBox_SelectionChanged;
            noisesComboBox.SelectionChanged += noisesComboBox_SelectionChanged;            

            minXTextBox.Tag = Parameter.Double("Левая граница", 0);
            maxXTextBox.Tag = Parameter.Double("Правая граница", 10);
            stepTextBox.Tag = Parameter.Double("Шаг", 0.1);
            kTextBox.Tag = Parameter.Double("Коэффицент", 1);

            minXTextBox.Text = (minXTextBox.Tag as Parameter).defaultValue.ToString();
            maxXTextBox.Text = (maxXTextBox.Tag as Parameter).defaultValue.ToString();
            stepTextBox.Text = (stepTextBox.Tag as Parameter).defaultValue.ToString();
            kTextBox.Text = (kTextBox.Tag as Parameter).defaultValue.ToString();

            refreshControlPanel();

            chart.LegendVisible = false;
        }        

        /// <summary>
        /// Обработчик кнопки "Генерировать"
        /// </summary>        
        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            logger.Info("Start generating");

            if (!validateInputs()) return;

            
            GenerationManager gm = new GenerationManager();

            IGenerator generator = pluginManager.getBeans(typeof(IGenerator)
                , (generatorsComboBox.SelectedItem as ComboBoxItem).Tag as string)
                [0] as IGenerator;

            INoise noise = null;
            if(noisesComboBox.SelectedItem != null)
            {
                noise = pluginManager.getBeans(typeof(INoise)
                , (noisesComboBox.SelectedItem as ComboBoxItem).Tag as string)
                [0] as INoise;
            }

            double minX = double.Parse(minXTextBox.Text)
                , maxX = double.Parse(maxXTextBox.Text)
                , step = double.Parse(stepTextBox.Text)
                , k = double.Parse(kTextBox.Text);

            try
            {
                generatedFunction = gm.generate(minX, maxX, step
                                        , generator, getParameters(generatorParameters)
                                        , noise, getParameters(noiseParameters)
                                        , k
                                        );
                drawFunction(generatedFunction);
            }
            catch(Exception ex)
            {
                logger.Error("Generation error: "+ex.ToString());
                MessageBox.Show("Ошибка генерирования");
            }
        }

        /// <summary>
        /// Заполняет ComboBox всеми доступными плагинами указанного типа
        /// </summary>
        /// <param name="comboBox">ComboBox для заполнения</param>
        /// <param name="pluginType">Тип плагина</param>
        private void  setupComboBox(ComboBox comboBox, Type pluginType)
        {
            IList<IPlugin> plugins = pluginManager.getBeans(pluginType);

            comboBox.Items.Clear();
            foreach(IPlugin plugin in plugins)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = plugin.title;
                item.Tag = plugin.name;
                comboBox.Items.Add(item);
            }

            comboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Формирует форму для заполнения параметров плагина
        /// </summary>
        /// <param name="stackPanel">StackPanel для заполнения</param>
        /// <param name="plugin">Плагин, для которого нужно сгенерировать форму</param>       
        private void setupParametersStackPanel(StackPanel stackPanel, IPlugin plugin)
        {
            stackPanel.Children.Clear();

            foreach(Parameter parameter in plugin.getParametersList())
            {
                /*
                 *<StackPanel Orientation="Horizontal">
                 *<TextBlock Text="name" Width="90" Margin="5" VerticalAlignment="Center"/>
                 *<TextBox Width="60" Margin="5"></TextBox>
                 *</StackPanel> 
                 */

                StackPanel horizontalStackPanel = new StackPanel();
                horizontalStackPanel.Orientation = Orientation.Horizontal;

                TextBlock title = new TextBlock();
                title.Text = parameter.title;
                title.Width = 90;
                title.Margin = new Thickness(5);
                title.VerticalAlignment = System.Windows.VerticalAlignment.Center;

                TextBox edit = new TextBox();
                edit.Text = parameter.defaultValue.ToString();
                edit.Width = 60;
                edit.Margin = new Thickness(5);
                edit.Tag = parameter;

                horizontalStackPanel.Children.Add(title);
                horizontalStackPanel.Children.Add(edit);

                stackPanel.Children.Add(horizontalStackPanel);
            }
        }

        /// <summary>
        /// Проверяет, что TextBox содержит допустимое значение
        /// </summary>        
        private bool validateTextBox(TextBox textBox)
        {
            if(textBox.Tag == null || !(textBox.Tag is Parameter))
            {
                logger.Error("There is no parameter assigned to textbox");
                return false;
            }

            Parameter parameter = textBox.Tag as Parameter;

            if (!parameter.validate(textBox.Text))
            {
                MessageBox.Show("Параметр \"" + parameter.title +"\" задан неверно");
                return false;
            }
            else return true;
        }

        /// <summary>
        /// Проверяет, что все TextBox-ы в StackPanel содержат допустимые значения
        /// </summary>        
        private bool validateTextBoxes(StackPanel stackPanel)
        {
            foreach (UIElement element in stackPanel.Children)
            {
                if (element is TextBox)
                    if (!validateTextBox(element as TextBox))
                        return false;
                if (element is StackPanel)
                    if (!validateTextBoxes(element as StackPanel))
                        return false;
            }

            return true;
        }

        /// <summary>
        /// Проверяет, что все поля содержат допустимые значения
        /// </summary>
        private bool validateInputs()
        {
            if (!validateTextBox(minXTextBox))
            {                
                return false;
            }
            if (!validateTextBox(maxXTextBox))
            {
                return false;
            }
            if (!validateTextBox(stepTextBox))
            {
                return false;
            }
            if (!validateTextBox(kTextBox))
            {
                return false;
            }

            if (!validateTextBoxes(generatorParameters)) return false;
            if (!validateTextBoxes(noiseParameters)) return false;            

            if (double.Parse(minXTextBox.Text) > double.Parse(maxXTextBox.Text))
            {
                string tmp = minXTextBox.Text;
                minXTextBox.Text = maxXTextBox.Text;
                maxXTextBox.Text = tmp;                
            }

            double step = double.Parse(stepTextBox.Text);              
            if (step < GlobalConstants.Precision)
            {
                MessageBox.Show("Шаг слишком мал");
                return false;
            }

            if(generatorsComboBox.SelectedItem == null)
            {
                MessageBox.Show("Не выбран генератор");
                return false;
            }

            IList<IPlugin> targetGenerator 
                = pluginManager.getBeans(typeof(IGenerator)
                    , (generatorsComboBox.SelectedItem as ComboBoxItem).Tag as string
                  );

            if(targetGenerator.Count != 1)
            {
                MessageBox.Show("Генератор не найден");
                return false;
            }

            IGenerator generator = targetGenerator[0] as IGenerator;            
            if(!generator.checkParametersList(getParameters(generatorParameters)))
            {
                MessageBox.Show("Неверные аргументы генератора");
                return false;
            }

            if (noisesComboBox.SelectedItem != null)
            {
                IList<IPlugin> targetNoise
                    = pluginManager.getBeans(typeof(INoise)
                        , (noisesComboBox.SelectedItem as ComboBoxItem).Content as string
                      );

                if (targetNoise.Count != 1)
                {
                    MessageBox.Show("Генератор не найден");
                    return false;
                }

                INoise noise = targetNoise[0] as INoise;
                if (!noise.checkParametersList(getParameters(noiseParameters)))
                {
                    MessageBox.Show("Неверные аргументы погрешности");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Перестраивает панель с формами
        /// </summary>
        private void refreshControlPanel()
        {
            setupComboBox(generatorsComboBox, typeof(IGenerator));            
            setupComboBox(noisesComboBox, typeof(INoise));                    
        }

        /// <summary>
        /// Отображает функцию f на графике
        /// </summary>
        private void drawFunction(Function f)
        {
            chart.LegendVisible = false;

            if (f == null) return;

            LinkedList<double> x = new LinkedList<double>(), y = new LinkedList<double>();

            for(double _x = f.minX; _x < f.maxX + f.step/2; _x+=f.step)
            {
                x.AddLast(_x);
                y.AddLast(f.getValue(_x));
            }                       
            
            var xDataSource = x.AsXDataSource();
            var yDataSource = y.AsYDataSource();

            CompositeDataSource compositeDataSource = xDataSource.Join(yDataSource);            
            functionGraph.DataSource = compositeDataSource;

            chart.FitToView();
        }

        /// <summary>
        /// Обработка изменения выбранного генератора
        /// </summary>
        private void generatorsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (sender as ComboBox);            
            if (comboBox.SelectedItem != null)
            {
                string generatorName = (comboBox.SelectedItem as ComboBoxItem).Tag as string;
                IList<IPlugin> target = pluginManager.getBeans(typeof(IGenerator), generatorName);
                if (target.Count != 1)
                {
                    logger.Fatal("Can not find selected generator");
                    throw new NullReferenceException("Can not find selected generator");
                }

                setupParametersStackPanel(generatorParameters, target[0]);
            }
        }

        /// <summary>
        /// Обработка изменения выбранной погрешности
        /// </summary>
        private void noisesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (sender as ComboBox);
            if (comboBox.SelectedItem != null)
            {
                string generatorName = (comboBox.SelectedItem as ComboBoxItem).Tag as string;
                IList<IPlugin> target = pluginManager.getBeans(typeof(INoise), generatorName);
                if (target.Count != 1)
                {
                    logger.Fatal("Can not find selected noise");
                    throw new NullReferenceException("Can not find selected noise");
                }

                setupParametersStackPanel(noiseParameters, target[0]);
            }
        }

        /// <summary>
        /// Получает список значений параметров из формы
        /// </summary>
        /// <param name="parameterStackPanel">Форма, которая будет обрабатываться</param>
        /// <returns>Список значений параметров из формы</returns>
        private IList<Object> getParameters(StackPanel parameterStackPanel)
        {
            IList<Object> result = new List<Object>();
            foreach (UIElement element in parameterStackPanel.Children)
            {
                if (element is TextBox)
                {
                    Parameter parameter = (element as TextBox).Tag as Parameter;
                    result.Add(parameter.parse((element as TextBox).Text));
                }

                if(element is StackPanel)
                {
                    foreach (Object p in getParameters(element as StackPanel))
                        result.Add(p);
                }
            }
            return result;
        }

        /// <summary>
        /// Сохраняет функцию в файл
        /// </summary>
        /// <param name="f">Функция</param>
        /// <param name="filename">Имя файла, куда следует сохранить функцию</param>
        private void saveToFile(Function f, string filename)
        {
            throw new System.NotImplementedException();
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
    }
}
