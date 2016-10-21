﻿/*
 * Copyright 2016 Mikhail Shiryaev
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * 
 * Product  : Rapid SCADA
 * Module   : ScadaWebCommon
 * Summary  : Web application settings
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2011
 * Modified : 2016
 */

using Scada.Client;
using Scada.Web.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Scada.Web
{
    /// <summary>
    /// Web application settings
    /// <para>Настройки веб-приложения</para>
    /// </summary>
    public class WebSettings : ISettings
    {
        /// <summary>
        /// Имя файла настроек по умолчанию
        /// </summary>
        public const string DefFileName = "WebSettings.xml";
        /// <summary>
        /// Мин. частота обновления данных, мс
        /// </summary>
        public const int MinDataRefrRate = 500;

        
        /// <summary>
        /// Конструктор
        /// </summary>
        public WebSettings()
        {
            CommSettings = new CommSettings();
            ScriptPaths = new ScriptPaths();
            PluginFileNames = new List<string>();
            SetToDefault();
        }


        /// <summary>
        /// Получить или установить культуру веб-приложения
        /// </summary>
        /// <remarks>Переопределяет культуру, заданную в реестре для всех приложений SCADA</remarks>
        public string Culture { get; set; }

        /// <summary>
        /// Получить или установить частоту обновления текущих данных и событий, мс
        /// </summary>
        public int DataRefrRate { get; set; }

        /// <summary>
        /// Получить или установить частоту обновления архивных данных, мс
        /// </summary>
        public int ArcRefrRate { get; set; }

        /// <summary>
        /// Получить или установить количество отображаемых событий
        /// </summary>
        public int DispEventCnt { get; set; }

        /// <summary>
        /// Получить или установить расстояние между разделяемыми точками графика, с
        /// </summary>
        public int ChartGap { get; set; }

        /// <summary>
        /// Получить или установить стартовую страницу после входа в систему
        /// </summary>
        public string StartPage { get; set; }

        /// <summary>
        /// Получить или установить разрешение команд управления
        /// </summary>
        public bool CmdEnabled { get; set; }

        /// <summary>
        /// Получить или установить необходимость ввода пароля при отправке команды
        /// </summary>
        public bool CmdPassword { get; set; }

        /// <summary>
        /// Получить или установить разрешение запоминать пользователя, вошедшего в систему
        /// </summary>
        public bool RemEnabled { get; set; }

        /// <summary>
        /// Получить или установить признак загрузки настроек представлений из базы конфигурации
        /// </summary>
        public bool ViewsFromBase { get; set; }

        /// <summary>
        /// Получить или установить разрешение передачи обезличенной статистики команде разработчиков
        /// </summary>
        public bool ShareStats { get; set; }


        /// <summary>
        /// Получить настройки соединения с сервером
        /// </summary>
        public CommSettings CommSettings { get; protected set; }

        /// <summary>
        /// Получить пути к дополнительным скриптам, реализующим функциональность оболочки
        /// </summary>
        public ScriptPaths ScriptPaths { get; protected set; }

        /// <summary>
        /// Получить имена файлов библиотек подключенных плагинов
        /// </summary>
        public List<string> PluginFileNames { get; protected set; }


        /// <summary>
        /// Установить значения настроек по умолчанию
        /// </summary>
        protected void SetToDefault()
        {
            Culture = "";
            DataRefrRate = 1000;
            ArcRefrRate = 10000;
            DispEventCnt = 100;
            ChartGap = 90;
            StartPage = "";
            CmdEnabled = true;
            CmdPassword = true;
            RemEnabled = false;
            ViewsFromBase = true;
            ShareStats = true;

            CommSettings.SetToDefault();
            ScriptPaths.SetToDefault();
            PluginFileNames.Clear();
        }


        /// <summary>
        /// Создать новый объект настроек
        /// </summary>
        public ISettings Create()
        {
            return new WebSettings();
        }

        /// <summary>
        /// Определить, равны ли заданные настройки текущим настройкам
        /// </summary>
        public bool Equals(ISettings settings)
        {
            return settings == this;
        }

        /// <summary>
        /// Загрузить настройки из файла
        /// </summary>
        public bool LoadFromFile(string fileName, out string errMsg)
        {
            // установка значений по умолчанию
            SetToDefault();

            // загрузка настроек
            try
            {
                if (!File.Exists(fileName))
                    throw new FileNotFoundException(string.Format(CommonPhrases.NamedFileNotFound, fileName));

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(fileName);
                XmlElement rootElem = xmlDoc.DocumentElement;

                // загрузка настроек соединения с сервером
                XmlNode commSettingsNode = rootElem.SelectSingleNode("CommSettings");
                if (commSettingsNode != null)
                    CommSettings.LoadFromXml(commSettingsNode);

                // загрузка общих параметров
                XmlNode commonParamsNode = rootElem.SelectSingleNode("CommonParams");
                if (commonParamsNode != null)
                {
                    XmlNodeList paramNodeList = commonParamsNode.SelectNodes("Param");
                    foreach (XmlElement paramElem in paramNodeList)
                    {
                        string name = paramElem.GetAttribute("name").Trim();
                        string nameL = name.ToLowerInvariant();
                        string val = paramElem.GetAttribute("value");

                        try
                        {
                            if (nameL == "culture")
                                Culture = val;
                            else if (nameL == "datarefrrate")
                                DataRefrRate = Math.Max(MinDataRefrRate, int.Parse(val));
                            else if (nameL == "arcrefrrate")
                                ArcRefrRate = Math.Max(MinDataRefrRate, int.Parse(val));
                            else if (nameL == "dispeventcnt")
                                DispEventCnt = int.Parse(val);
                            else if (nameL == "chartgap")
                                ChartGap = int.Parse(val);
                            else if (nameL == "cmdenabled")
                                CmdEnabled = bool.Parse(val);
                            else if (nameL == "cmdpassword")
                                CmdPassword = bool.Parse(val);
                            else if (nameL == "remenabled")
                                RemEnabled = bool.Parse(val);
                            else if (nameL == "startpage")
                                StartPage = val;
                            else if (nameL == "viewsfrombase")
                                ViewsFromBase = bool.Parse(val);
                            else if (nameL == "sharestats")
                                ShareStats = bool.Parse(val);
                        }
                        catch
                        {
                            throw new ScadaException(string.Format(CommonPhrases.IncorrectXmlParamVal, name));
                        }
                    }
                }

                // загрузка путей к скриптам
                XmlNode scriptPathsNode = rootElem.SelectSingleNode("ScriptPaths");
                if (scriptPathsNode != null)
                {
                    XmlNodeList scriptNodeList = scriptPathsNode.SelectNodes("Script");
                    foreach (XmlElement scriptElem in scriptNodeList)
                    {
                        string name = scriptElem.GetAttribute("name").Trim();
                        string nameL = name.ToLowerInvariant();
                        string path = scriptElem.GetAttribute("path");

                        if (nameL == "chartscript")
                            ScriptPaths.ChartScriptPath = path;
                        else if (nameL == "cmdscript")
                            ScriptPaths.CmdScriptPath = path;
                        else if (nameL == "eventackscript")
                            ScriptPaths.EventAckScriptPath = path;
                    }
                }

                // загрузка имён файлов модулей
                XmlNode pluginsNode = rootElem.SelectSingleNode("Plugins");
                if (pluginsNode != null)
                {
                    XmlNodeList moduleNodeList = pluginsNode.SelectNodes("Plugin");
                    foreach (XmlElement moduleElem in moduleNodeList)
                        PluginFileNames.Add(moduleElem.GetAttribute("fileName"));
                }

                errMsg = "";
                return true;
            }
            catch (Exception ex)
            {
                errMsg = WebPhrases.LoadWebSettingsError + ": " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Сохранить настройки в файле
        /// </summary>
        public bool SaveToFile(string fileName, out string errMsg)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();

                XmlDeclaration xmlDecl = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                xmlDoc.AppendChild(xmlDecl);

                XmlElement rootElem = xmlDoc.CreateElement("WebSettings");
                xmlDoc.AppendChild(rootElem);

                // настройки соединения
                XmlElement commSettingsElem = xmlDoc.CreateElement("CommSettings");
                rootElem.AppendChild(commSettingsElem);
                CommSettings.SaveToXml(commSettingsElem);

                // общие параметры
                XmlElement commonParamsElem = xmlDoc.CreateElement("CommonParams");
                rootElem.AppendChild(commonParamsElem);

                commonParamsElem.AppendParamElem("Culture", Culture,
                    "Культура веб-приложения", "Web application culture");
                commonParamsElem.AppendParamElem("DataRefrRate", DataRefrRate,
                    "Частота обновления текущих данных и событий, мс", "Current data and events refresh rate, ms");
                commonParamsElem.AppendParamElem("ArcRefrRate", ArcRefrRate,
                    "Частота обновления архивных данных, мс", "Archive data refresh rate, ms");
                commonParamsElem.AppendParamElem("DispEventCnt", DispEventCnt,
                    "Количество отображаемых событий", "Display events count");
                commonParamsElem.AppendParamElem("ChartGap", ChartGap,
                    "Расстояние между точками графика для разрыва, с", "Distance between chart points to make a gap, sec");
                commonParamsElem.AppendParamElem("StartPage", StartPage,
                    "Начальная страница после входа в систему", "Start page after login");
                commonParamsElem.AppendParamElem("CmdEnabled", CmdEnabled,
                    "Разрешить команды ТУ", "Enable commands");
                commonParamsElem.AppendParamElem("CmdPassword", CmdPassword,
                    "Требовать пароль при отправке команды", "Require password to send command");
                commonParamsElem.AppendParamElem("RemEnabled", RemEnabled,
                    "Разрешить запоминать вход пользователя", "Allow to remember logged on user");
                commonParamsElem.AppendParamElem("ViewsFromBase", ViewsFromBase,
                    "Загружать настройки представлений из базы", "Load view settings from the database");
                commonParamsElem.AppendParamElem("ShareStats", ShareStats,
                    "Поделиться обезличенной статистикой с разработчиками", "Share depersonalized stats with the developers");

                // пути к скриптам
                XmlElement scriptPathsElem = xmlDoc.CreateElement("ScriptPaths");
                rootElem.AppendChild(scriptPathsElem);

                XmlElement scriptElem = xmlDoc.CreateElement("Script");
                scriptElem.SetAttribute("name", "ChartScript");
                scriptElem.SetAttribute("path", ScriptPaths.ChartScriptPath);
                scriptPathsElem.AppendChild(scriptElem);

                scriptElem = xmlDoc.CreateElement("Script");
                scriptElem.SetAttribute("name", "CmdScript");
                scriptElem.SetAttribute("path", ScriptPaths.CmdScriptPath);
                scriptPathsElem.AppendChild(scriptElem);

                scriptElem = xmlDoc.CreateElement("Script");
                scriptElem.SetAttribute("name", "EventAckScript");
                scriptElem.SetAttribute("path", ScriptPaths.EventAckScriptPath);
                scriptPathsElem.AppendChild(scriptElem);

                // плагины
                XmlElement pluginsElem = xmlDoc.CreateElement("Plugins");
                rootElem.AppendChild(pluginsElem);

                foreach (string pluginFileName in PluginFileNames)
                {
                    scriptElem = xmlDoc.CreateElement("Plugin");
                    scriptElem.SetAttribute("fileName", pluginFileName);
                    pluginsElem.AppendChild(scriptElem);
                }

                // сохранение в файле
                string bakName = fileName + ".bak";
                File.Copy(fileName, bakName, true);
                xmlDoc.Save(fileName);

                errMsg = "";
                return true;
            }
            catch (Exception ex)
            {
                errMsg = WebPhrases.SaveWebSettingsError + ": " + ex.Message;
                return false;
            }
        }
    }
}
