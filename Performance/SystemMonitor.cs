using System;
using System.Linq;
using System.Management;
using System.Timers;
using Microsoft.VisualBasic.Devices;

namespace Performance
{
    public class SystemMonitor : IDisposable
    {
        //Таймер, по истечению которого будет считана информация о производительности.
        private readonly Timer _timer = new Timer();
        //Объект, при помощи которого можно определить доступную и полную оперативную память компьютера.
        private readonly ComputerInfo _computerInfo = new ComputerInfo();
        /// <summary>
        /// Событие, которое наступает, когда информация о производительности считана и сформирована в класс Performance.
        /// </summary>
        public event EventHandler<Performance> GotPerformance;

        /// <summary>
        /// Интервал, по истечению которого будет проводиться считывание данных о производительности.
        /// </summary>
        public double GetPerformanceInterval
        {
            get { return _timer.Interval; }
            set { _timer.Interval = value; }
        }

        /// <summary>
        /// Конструктор класса с параметрами по умолчанию.
        /// </summary>
        /// <param name="interval"></param>
        public SystemMonitor(double interval = 60*60*1000)
        {
            _timer.Interval = interval;
            _timer.Elapsed += GetPerformance;
        }

        /// <summary>
        /// Метод, запускающий работу класса.
        /// </summary>
        public void Start()
        {
            _timer.Start();
        }
        /// <summary>
        /// Метод, останавливающий работу класса.
        /// </summary>
        public void Stop()
        {
            _timer.Stop();
        }

        //Свойство, вызов которого повлечёт за собой определение загруженности процессора.
        private ushort ProcessorLoad
        {
            get
            {
                var processor = new SelectQuery("SELECT * FROM CIM_Processor");
                using (var searcher = new ManagementObjectSearcher(processor))
                    return (ushort) searcher.Get()
                        .Cast<ManagementObject>()
                        .Select(obj => obj["LoadPercentage"])
                        .First();
            }
        }
        //Свойство, вызов которого повлечёт за собой определение доступной оперативной памяти компьютера.
        private ulong AvailablePhysicalMemory
        {
            get { return _computerInfo.AvailablePhysicalMemory; }
        }

        //Свойство, вызов которого повлечёт за собой определение полной оперативной памяти компьютера.
        private ulong TotalPhysicalMemory
        {
            get
            {
                return _computerInfo.TotalPhysicalMemory;
            }
        }

        //Свойство, вызов которого повлечёт за собой определение объёма первого жёсткого диска.
        private ulong DiskCapacity
        {
            get
            {
                var volumes = new SelectQuery("SELECT * FROM Win32_Volume");
                using (var searcher = new ManagementObjectSearcher(volumes))
                    return (ulong) searcher.Get()
                        .Cast<ManagementObject>()
                        .Select(queryObj => queryObj["Capacity"])
                        .First();
            }
        }

        //Свойство, вызов которого повлечёт за собой определение свободного пространства первого жёсткого диска.
        private ulong DiskFreeSpace
        {
            get
            {
                var volumes = new SelectQuery("SELECT * FROM Win32_Volume");
                using (var searcher = new ManagementObjectSearcher(volumes))
                    return (ulong) searcher.Get()
                        .Cast<ManagementObject>()
                        .Select(queryObj => queryObj["FreeSpace"])
                        .First();
            }
        }

        //Метод, который отрабатывается всякий раз, когд интервал таймера истёк.
        private void GetPerformance(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            var currentPerformance = new Performance(DateTime.Now, ProcessorLoad, AvailablePhysicalMemory,
                TotalPhysicalMemory, DiskCapacity, DiskFreeSpace);
            OnGotPerformance(currentPerformance);
        }

        //Метод, вызывающий событие GotPerformance.
        protected virtual void OnGotPerformance(Performance e)
        {
            GotPerformance?.Invoke(this, e);
        }

        //Метод, освобождающий использованные ресурсы данного класса.
        public void Dispose()
        {
            GotPerformance = null;
            _timer.Stop();
        }
    }
}