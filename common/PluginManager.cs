using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Reflection;
using NLog;

namespace Common
{
    /// <summary>
    /// PluginManager загружает плагины из сборок
    /// </summary>
    public class PluginManager
    {
        IList<IPlugin> beans = new List<IPlugin>();
        static Logger logger = LogManager.GetCurrentClassLogger();
        static PluginManager singleton = null;          

        /// <summary>
        /// Конструктор по умолчанию класса PluginManager
        /// </summary>
        private PluginManager()
        {
            refresh();
        }

        /// <summary>
        /// Возвращает объект класса PluginManager
        /// </summary>
        /// <returns>Объект PluginManager</returns>
        public static PluginManager getPluginManager()
        {
            if (singleton == null) singleton = new PluginManager();

            return singleton;
        }

        /// <summary>
        /// Возвращает все плагины, которые реализуют интерфейс interfaceType и имеют имя plaginName
        /// </summary>
        /// <param name="interfaceType">Интерфейс, который должен реализовывать плагин</param>
        /// <param name="plaginName">Имя плагина. Если plaginName равен null, имя игнорируется</param>
        /// <returns>Список плагинов, которые реализуют интерфейс interfaceType и имеют имя plaginName</returns>
        public virtual IList<IPlugin> getBeans(Type interfaceType, string plaginName = null)
        {
            IList<IPlugin> result = new List<IPlugin>();

            foreach(IPlugin plugin in beans)
            {                
                if (plugin.GetType().GetInterface(interfaceType.FullName) != null)
                {
                    if (plaginName == null || plaginName.Equals(plugin.name))
                        result.Add(plugin);
                }
            }

            return result;
        }

        /// <summary>
        /// Загружает все плагины из сборок
        /// </summary>
        public virtual void refresh()
        {            
            beans.Clear();

            foreach (string dllFileName in Directory.GetFileSystemEntries("plugins/", "*.dll"))            
                
                try
                {
                    Assembly ass = Assembly.LoadFrom(dllFileName);
                    //Цикл по всем типам в DLL
                    foreach (Type objType in ass.GetTypes())
                    {
                        //Смотрим только типы public
                        if (objType.IsPublic == true)
                        {
                            //игнорируем абстрактные классы                            
                            if (objType.Attributes != TypeAttributes.Abstract)
                            {
                                //Смотрим, реализует ли этот тип интерфейс IPlugin
                                Type objInterface = objType.GetInterface(typeof(IPlugin).FullName);
                                if (objInterface != null)
                                {
                                    IPlugin plugin = (IPlugin)ass.CreateInstance(objType.FullName);
                                    plugin.initialize();
                                    beans.Add(plugin);

                                    logger.Trace("Loaded class " + objType.FullName);
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Plugin loading fault: " + ex.ToString());
                }            
        }

    }
}
