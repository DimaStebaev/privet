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
                UIElement result = pm.process(processedFunction);

                resultControl.Content = result;
            }
            catch(Exception ex)
            {
                logger.Error("Procession error: " + ex.ToString());
                MessageBox.Show("Ошибка обработки");
            }
        }

#endregion
#endregion      

        
    }
}
