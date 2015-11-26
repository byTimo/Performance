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
                Console.WriteLine($"Сервер сконфигурирован. Порт {Server.Port}, максимальное количество подключений {Server.MaxConnection}.");
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
                Console.WriteLine($"Аварийное завершение работы программы! Ошибка: {e.Message}");
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
            Console.WriteLine($"Получена производительность {performance.Time}.");
            try
            {
                DbManager.Instance.Performances.Add(performance);
                DbManager.Instance.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Ошибка при добавлении информации в базу данных. Ошибка: {e.Message}");
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
                Console.WriteLine($"Файл {settingDocument} не был найден.");
                throw;
            }
        }

        private static int GetElementAttributeValue(XmlDocument document, string elementName, string attribName)
        {
            try
            {
                var element = document.DocumentElement[elementName];
                if (element == null)
                    throw new Exception($"Невозможно определить значение {elementName} из файла настроек.");
                var attrib = element.Attributes[attribName];
                if (attrib == null)
                    throw new Exception(
                        $"Невозможно определить аттрибут {attribName} для элемента {elementName} из файла настроек.");
                int value;
                if(!int.TryParse(attrib.Value, out value))
                    throw new Exception($"Не корректно задано значение аттрибута {attribName} элемента {elementName} в файле настроек.");
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
