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
using Microsoft.Win32;
using NLog;

using Common;

namespace Processor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();   
         
            refreshControlPanel();
        }
#region Fields

        PluginSelector processorSelector = null;
        static Logger logger = LogManager.GetCurrentClassLogger();
        Function _processedFunction = null;
        PluginManager pluginManager = PluginManager.getPluginManager();

        Function processedFunction
        {
            get
            {
                return _processedFunction;
            }
            set
            {
                _processedFunction = value;

                if(_processedFunction == null)
                {
                    generateButton.IsEnabled = false;
                }
                else
                {
                    generateButton.IsEnabled = true;
                }
            }
        }

#endregion

#region Private methods

        /// <summary>
        /// Пересоздаёт панель управления
        /// </summary>
        private void refreshControlPanel()
        {
            processorSelector = new PluginSelector(typeof(IProcessor));
            processorSelectorControl.Content = processorSelector;

            processedFunction = processedFunction;
        }

        private bool validateInputs()
        {
            IList<string> errors = processorSelector.getErrors();

            if (errors.Count > 0)
            {
                string errorMessage = "";
                foreach (string line in errors)
                {
                    if (errorMessage != "") errorMessage += Environment.NewLine;
                    errorMessage += line;
                }
                MessageBox.Show(errorMessage);
                return false;
            }
            else
                return true;
        }

#region Buttons handles

        /// <summary>
        /// Обработчик кнопки "Обработать"
        /// </summary>        
        private void processButton_Click(object sender, RoutedEventArgs e)
        {
            if (!validateInputs()) return;            

            try
            {
                IProcessor processor = processorSelector.getSelectedPlugin() as IProcessor;
                processor.setup(processorSelector.getPluginParametersValues());

                ProcessingManager pm = new ProcessingManager();
                UIElement result = pm.process(processor, processedFunction);

                resultControl.Content = result;
            }
            catch(Exception ex)
            {
                logger.Error("Procession error: " + ex.ToString());
                MessageBox.Show("Ошибка обработки");
            }
        }

        /// <summary>
        /// Обработчик нажатия на пункт меню "Выход"
        /// </summary>        
        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Обработчик нажатия на пункт меню "Загрузить данные..."
        /// </summary>  
        private void MenuItemLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            IList<IPlugin> plugins = pluginManager.getBeans(typeof(ISerializer));

            if (plugins.Count < 1)
            {
                MessageBox.Show("Невозвожно загрузить функцию, так как отсутствуют соответствующие плагины");
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
                        processedFunction = s.deserialize(filename);                        
                        return;
                    }
                }

                logger.Error("Loading function error");
                MessageBox.Show("Ошибка во время загрузки данных");
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

#endregion
#endregion      

        
    }
}
