using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace Common
{   
    /// <summary>
    /// Отображает список доступных плагинов и форму для ввода их параметров
    /// </summary>
    public class PluginSelector : StackPanel
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
            foreach (IPlugin plugin in plugins)
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
            if (comboBox.SelectedItem == null) return null;
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
            if (comboBox.SelectedItem == null) return;

            IPlugin selectedPlugin = (comboBox.SelectedItem as ComboBoxItem).Tag as IPlugin;

            form = new Form(selectedPlugin.getParametersList());

            parametersForm.Content = form;
        }
        #endregion
    }
}
