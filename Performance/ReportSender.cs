using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using Newtonsoft.Json;

namespace Performance
{
    public class ReportSender : IDisposable
    {
        //Переменная показывает, освободил ли класс ресурсы, которые он использовал.
        private bool _isDisposed;
        //Специальный объект, по которому определяется, заблокирован ли в данный момент список неотправленных пакетов
        private readonly object _listBlocker = new object();
        //Таймер отмеряющий интервал времени, через который будет совершена попытка отправки неотправленных пакетов
        private Timer _sendingTimer = new Timer();
        //Список неотправленных пакетов
        private readonly List<byte[]> _unsentPackets = new List<byte[]>();

        /// <summary>
        /// Интервал таймера, через который будет осуществлена новая попытка отправки пакетов.
        /// </summary>
        public double IntervalAttempts
        {
            get { return _sendingTimer.Interval; }
            set { _sendingTimer.Interval = value; }
        }

        /// <summary>
        /// Конечная точка, определяющая адрес и порт сервера в сети.
        /// </summary>
        public IPEndPoint ServerEndPoint { get; set; } = new IPEndPoint(IPAddress.Loopback, 80);

        /// <summary>
        /// Конструктор класса, выставляющий все параметры по умолчанию.
        /// </summary>
        public ReportSender()
        {
            IntervalAttempts = 30000;
            _sendingTimer.Elapsed += TimerStart;
        }

        /// <summary>
        /// Метод осуществляет отправку информации о производительности. При неудачной отправке пакет записывается в список неотправленных пакетов
        /// и включается таймер повторной отправки.
        /// </summary>
        /// <param name="performance">Информация о производительности переданная через класс Performance.</param>
        public void SendPerformance(Performance performance)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var jsonObj = JsonConvert.SerializeObject(performance);
            var buffer = GetPostMessage(jsonObj);
            try
            {
                socket.Connect(ServerEndPoint);
                socket.Send(buffer);
                var responceBuffer = new byte[1024];
                var received = socket.Receive(responceBuffer);
                var responce = Encoding.UTF8.GetString(responceBuffer, 0, received);
                if (!responce.EndsWith("OK"))
                    throw new Exception("Сервер не подтвердил получение");
                Console.WriteLine("Пакет отправлен успешно.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                lock (_unsentPackets)
                {
                    _unsentPackets.Add(buffer);
                    if (!_isDisposed && !_sendingTimer.Enabled)
                        _sendingTimer.Start();
                }
            }
            finally
            {
                if(socket.Connected)
                    socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                socket.Dispose();
            }
        }

        //Метод, который отрабатывается всякий раз, когда интервал таймера истечёт.
        private void TimerStart(object sender, ElapsedEventArgs e)
        {
            if(!_isDisposed)
                _sendingTimer.Stop();
            if (_unsentPackets.Count == 0)
                return;
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Console.WriteLine($"Попытка отправить {_unsentPackets.Count} неотправленных пакетов.");
            lock (_listBlocker)
            {
                try
                {
                    socket.Connect(ServerEndPoint);
                    foreach (var unsentPacket in _unsentPackets)
                    {
                        socket.Send(unsentPacket);
                        var responceBuffer = new byte[1024];
                        var received = socket.Receive(responceBuffer);
                        var responce = Encoding.UTF8.GetString(responceBuffer, 0, received);
                        if (!responce.EndsWith("OK"))
                            throw new Exception("Сервер не подтвердил получение.");
                    }
                    socket.Disconnect(true);
                    if (socket.Connected)
                        socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                    socket.Dispose();
                    Console.WriteLine($"{_unsentPackets.Count} отправлены успешно.");
                }
                catch (Exception ex)
                {
                    if (socket.Connected)
                        socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                    socket.Dispose();
                    Console.WriteLine(ex.Message);
                    if(!_isDisposed)
                        _sendingTimer.Start();
                }
            }
        }

        //Метод возращающий HTTP пакет с типом запроса POST в формате массива байт.
        private static byte[] GetPostMessage(string message)
        {
            var messageStringBuilder = new StringBuilder();
            messageStringBuilder.Append("POST HTTP/1.1\r\nContent-Type: application/x-www-form-urlencoded\r\n");
            messageStringBuilder.Append($"Content-Length: {message.Length}");
            messageStringBuilder.Append("\r\n\r\n");
            messageStringBuilder.Append(message);
            return Encoding.UTF8.GetBytes(messageStringBuilder.ToString());
        }

        //Метод, освобождающий ресурсы, использованные данным классом.
        public void Dispose()
        {
            _isDisposed = true;

            if(_sendingTimer.Enabled)
                _sendingTimer.Stop();
            _sendingTimer.Dispose();
            _sendingTimer = null;
        }
    }
}