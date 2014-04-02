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

            windowCaption = "";

            refreshControlPanel();
        }
#region Fields
        
        static Logger logger = LogManager.GetCurrentClassLogger();
        Function _processedFunction = null;
        PluginManager pluginManager = PluginManager.getPluginManager();

        Function processedFunction
        {
            get { return _processedFunction; }
            set {
                _processedFunction = value;
                resultControl.Content = null;
            }
        }

        string windowCaption
        {
            set { 
                this.Title = "Обработчик";
                if (value != null && value != "")
                    this.Title += " - " + value;
            }
        }

#endregion

#region Private methods

        /// <summary>
        /// Пересоздаёт панель управления
        /// </summary>
        private void refreshControlPanel()
        {
            StackPanel sp = new StackPanel();
            
            foreach(IPlugin processor in pluginManager.getBeans(typeof(IProcessor)))
            {
                if (processor == null) continue;

                CheckBox chbx = new CheckBox();
                chbx.Content = processor.title;

                chbx.IsChecked = true;
                chbx.Tag = processor;

                chbx.Margin = new Thickness(5);

                sp.Children.Add(chbx);                
            }
            
            processorSelectorControl.Content = sp;            
        }        

        /// <summary>
        /// Возвращает список процессорво, которые выбрал пользователь в UI.
        /// </summary>        
        private List<IProcessor> getSelectedProcessors()
        {
            List<IProcessor> selectedProcessors = new List<IProcessor>();

            if (!(processorSelectorControl.Content is StackPanel)) return selectedProcessors;



            foreach (UIElement element in (processorSelectorControl.Content as StackPanel).Children)
                if (element is CheckBox && element != null)
                    if ((bool)(element as CheckBox).IsChecked)
                        selectedProcessors.Add((element as CheckBox).Tag as IProcessor);

            return selectedProcessors;
        }

#region Buttons handles

        /// <summary>
        /// Обработчик кнопки "Обработать"
        /// </summary>        
        private void processButton_Click(object sender, RoutedEventArgs e)
        {
            IList<IProcessor> selectedProcessors = getSelectedProcessors();
            if(selectedProcessors.Count == 0)
            {
                MessageBox.Show("Не выбрано ни одного обработчика");
                return;
            }

            if(processedFunction == null)
                MenuItemLoad_Click(this, new RoutedEventArgs());

            if (processedFunction == null)
                return;

            try
            {
                StackPanel resultStackPanel = new StackPanel();

                foreach(IProcessor processor in selectedProcessors)
                {
                    processor.setup(new List<Object>());

                    ProcessingManager pm = new ProcessingManager();
                    UIElement result = pm.process(processor, processedFunction);

                    resultStackPanel.Children.Add(result);
                }                                

                resultControl.Content = resultStackPanel;
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

                        windowCaption = "загружен файл " + filename;

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
