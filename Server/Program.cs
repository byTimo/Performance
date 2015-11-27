using System;
using System.IO;
using System.Xml;

namespace Server
{
    class Program
    {
        private static HttpServer Server { get; set; }

        static void Main()
        {
            var isRunning = true;
            try
            {
                ConfigureServer("Settings.xml");
                Console.WriteLine("Сервер сконфигурирован. Порт {0}, максимальное количество подключений {1}.", Server.Port, Server.MaxConnection);
                Server.ReceivedPerformance += ReceivePerformance;
                Server.Start();
                Console.WriteLine("Сервер запущен.");
                Console.WriteLine("Для завршения работы сервера нажите клавишу Esc.");
                while (isRunning)
                {
                    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                        isRunning = false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Аварийное завершение работы программы! Ошибка: {0}", e.Message);
                Console.WriteLine("Нажимте любую клавишу.");
                Console.ReadKey();
            }
            finally
            {
                Server.Dispose();
            }
        }
        
        private static void ReceivePerformance(object sender, Performance performance)
        {
            Console.WriteLine("Получена производительность {0}.", performance.Time);
            try
            {
                DbManager.Instance.Performances.Add(performance);
                DbManager.Instance.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка при добавлении информации в базу данных. Ошибка: {0}", e.Message);
            }
        }

        private static void ConfigureServer(string settingFilePath)
        {
            Server = new HttpServer();
            try
            { 
                var settingDocument = ReadSettingXmlFile(settingFilePath);
                Server.Port = GetElementAttributeValue(settingDocument, "port", "value");
                Server.MaxConnection = GetElementAttributeValue(settingDocument, "max_connection", "value");
            }
            catch (Exception)
            {
                Console.WriteLine("Сервер сконфигурирован с настройками по умолчанию.");
            }

        }

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

        private static int GetElementAttributeValue(XmlDocument document, string elementName, string attribName)
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
                int value;
                if(!int.TryParse(attrib.Value, out value))
                    throw new Exception(string.Format("Не корректно задано значение аттрибута {0} элемента {1} в файле настроек.", attribName, elementName));
                return value;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
    }
}
