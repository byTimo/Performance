using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace Server
{
    public class HttpServer : IDisposable
    {
        //Порт, на котором будет работать сервер.
        private int _port;
        //Количество возможных одновременных подключений к серверу.
        private int _maxConnection = 2;
        //Ссылка на поток, в котором будет осуществляться прослушка сокета.
        private Thread _listeningThread;

        //Свойсто указывающее запущен ли сервер.
        public bool IsRunning { get; private set; }

        //Свойсто, позволяющее переопределить порт сервера.
        public int Port
        {
            get { return _port; }
            set
            {
                if(IsRunning)
                    throw new Exception("Нельзя менять порт, когда сервер уже запущен!");
                _port = value;
            }
        }
        //Свойство, позволяющее переопределить максимальное количество одновременных подключений.
        public int MaxConnection
        {
            get { return _maxConnection; }
            set
            {
                if (IsRunning)
                    throw new Exception("Нельзя менять максимальное количество подключений, когда сервер уже запущен!");
                _maxConnection = value;
            }
        }

        //Сокет, прослушку которого осуществляет сервер.
        public Socket ServerSocket { get; }

        /// <summary>
        /// Событие, возникающее когда сервер получил и обработал пакет с информацией о производительности.
        /// </summary>
        public event EventHandler<Performance> ReceivedPerformance;
        
        //Конструктор, задающий стандартные параметры.
        public HttpServer(int port = 80)
        {
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Port = port;
        }

        //Метод запускает работу сервера.
        public void Start()
        {
            ServerSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
            ServerSocket.Listen(MaxConnection);
            _listeningThread = new Thread(ListeningHandler);
            IsRunning = true;
            _listeningThread.Start();
        }

        //Метод, который вполняется в потоке прослушивания.
        private void ListeningHandler()
        {
            while (IsRunning)
            {
                var connection = ServerSocket.Accept();
                var receiveThread = new Thread(ReceivePerformance);
                receiveThread.Start(connection);
                Thread.Sleep(500);
            }
        }

        //Метод, который выполняется в потоке нового подключения сервера.
        private void ReceivePerformance(object connection)
        {
            var clientSocket = (Socket)connection;
            try
            {
                while (clientSocket.Available != 0)
                {
                    var buffer = new byte[1024];
                    var length = clientSocket.Receive(buffer);
                    var request = Encoding.UTF8.GetString(buffer, 0, length);
                    var jsonObj = request.Substring(request.IndexOf("\r\n\r\n", StringComparison.Ordinal) + 4);
                    var performance = JsonConvert.DeserializeObject<Performance>(jsonObj);
                    var responce = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK");
                    clientSocket.Send(responce);
                    OnReceivedPerformance(performance);
                    Thread.Sleep(500);
                }

            }
            catch (Exception)
            {
                Console.WriteLine("Ошибка при получении. Пакет игнорируется.");
            }
            finally
            {
                if(clientSocket.Connected)
                    clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
        }

        //Метод, вызывающий событие получения информации о производительности.
        protected virtual void OnReceivedPerformance(Performance e)
        {
            ReceivedPerformance?.Invoke(this, e);
        }

        /// <summary>
        /// Освобождение ресурсов.
        /// </summary>
        public void Dispose()
        {
            ServerSocket.Close();
            ServerSocket.Dispose();
            if (_listeningThread == null) return;
            _listeningThread.Abort();
            _listeningThread.Join();
            _listeningThread = null;
        }
    }
}