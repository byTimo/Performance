using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Xml;

namespace Performance
{
    internal class Program
    {
        //Модуль, необходимый для считывания данных о производительности системы
        private static SystemMonitor MonitorModule { get; set; }
        //Модуль, необходимый для отправки информации о производительности системы
        private static ReportSender SenderModule { get; set; }

        public static void Main(string[] args)
        {
            var isRunning = true;
            ConfigureModules("Settings.xml");
            Console.WriteLine($"Программа сконфигурирована.\nИнтервал получения данных {MonitorModule.GetPerformanceInterval} мс ({MonitorModule.GetPerformanceInterval/1000} с).");
            Console.WriteLine($"Адресс сервера {SenderModule.ServerEndPoint}");
            Console.WriteLine($"Интервал повоторных попыток: {SenderModule.IntervalAttempts}");
            MonitorModule.GotPerformance += SendPerformance;
            MonitorModule.Start();
            Console.WriteLine("Запущен анализирующий модуль.");
            while (isRunning)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                    isRunning = false;
                Thread.Sleep(500);
            }
            SenderModule.Dispose();
            MonitorModule.Dispose();
            Thread.Sleep(1000);
        }
        
        //Метод, который вызывается всякий раз, когда наступает событие считывания информации о производительности
        private static void SendPerformance(object sender, Performance performance)
        {
            Console.WriteLine("Сформирован отчёт от {0}", performance.Time);
            SenderModule.SendPerformance(performance);
        }

        //Метод, необходимый для конфигурации модулей.
        private static void ConfigureModules(string settingFilePath)
        {
            MonitorModule = new SystemMonitor();
            SenderModule = new ReportSender();
            try
            {
                var settingDocument = ReadSettingXmlFile(settingFilePath);
                MonitorModule.GetPerformanceInterval = GetAttributeValueDouble(settingDocument, "monitor_interval",
                    "value");
                var serverAddress =
                    Dns.GetHostAddresses(GetElementAttribute(settingDocument, "server_name", "value").Value)
                        .First(ad => ad.AddressFamily == AddressFamily.InterNetwork);
                var serverPort = GetAttributeValueInt32(settingDocument, "server_port", "value");
                SenderModule.ServerEndPoint = new IPEndPoint(serverAddress, serverPort);
                SenderModule.IntervalAttempts = GetAttributeValueDouble(settingDocument, "interval_attempt", "value");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Сервер сконфигурирован с настройками по умолчанию.");
            }

        }

        //Метод, считывающий данные из XML файла и создающий специальный класс для работы с этими данными.
        private static XmlDocument ReadSettingXmlFile(string settingFilePath)
        {
            var settingDocument = new XmlDocument();
            try
            {
                settingDocument.Load(settingFilePath);
                return settingDocument;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Файл {0} не был найден.", settingDocument);
                throw;
            }
        }

        //Метод, позволяющий получить значение заданного параметра заданного элемента в XML файле. Значение приводится к типу Int32.
        private static int GetAttributeValueInt32(XmlDocument document, string elementName, string attribName)
        {
            try
            {
                var attrib = GetElementAttribute(document, elementName, attribName);
                int value;
                if (!int.TryParse(attrib.Value, out value))
                    throw new Exception(string.Format("Не корректно задано значение аттрибута {0} элемента {1} в файле настроек.", attribName, elementName));
                return value;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
        //Метод, позволяющий получить значение заданного параметра заданного элемента в XML файле. Значение приводится к типу Double.
        private static double GetAttributeValueDouble(XmlDocument document, string elementName, string attribName)
        {
            try
            {
                var attrib = GetElementAttribute(document, elementName, attribName);
                double value;
                if (!double.TryParse(attrib.Value, out value))
                    throw new Exception(string.Format("Не корректно задано значение аттрибута {0} элемента {1} в файле настроек.", attribName, elementName));
                return value;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
        //Метод, позволяющий получить значение заданного параметра заданного элемента в XML файле.
        private static XmlAttribute GetElementAttribute(XmlDocument document, string elementName, string attribName)
        {
            try
            {
                var element = document.DocumentElement[elementName];
                if (element == null)
                    throw new Exception(string.Format("Невозможно определить значение {0} из файла настроек.", elementName));
                var attrib = element.Attributes[attribName];
                if (attrib == null)
                    throw new Exception(
                       string.Format("Невозможно определить аттрибут {0} для элемента {1} из файла настроек.", attribName, elementName));
                return attrib;
            }
            catch
            {
                throw;
            }
        }
    }
}