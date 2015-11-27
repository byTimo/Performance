using System;
using System.Text;

namespace Performance
{
    /// <summary>
    /// Класс, отображающий информацию о производительности компьютера.
    /// </summary>
    public class Performance
    {
        public DateTime Time { get; }
        public string MachineIdentifier { get; }
        public ushort ProcessorLoad { get;  }
        public ulong AvailablePhysicalMemory { get; }
        public ulong TotalPhysicalMemory { get; }
        public ulong DiskCapacity{ get; }
        public ulong DiskFreeSpace { get; }

        public Performance(DateTime time, ushort procLoad, ulong availPm, ulong totalPm, ulong capacity, ulong freeSpace)
        {
            Time = time;
            MachineIdentifier = Environment.MachineName;
            ProcessorLoad = procLoad;
            AvailablePhysicalMemory = availPm;
            TotalPhysicalMemory = totalPm;
            DiskCapacity = capacity;
            DiskFreeSpace = freeSpace;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(string.Format("Время: {0}\n", Time));
            builder.Append(string.Format("ID: {0}\n", MachineIdentifier));
            builder.Append(string.Format("Загруженность процессора: {0}\n", ProcessorLoad));
            builder.Append(string.Format("Доступная оперативная память: {0}\n", AvailablePhysicalMemory));
            builder.Append(string.Format("Полная оперативная память: {0}\n", TotalPhysicalMemory));
            builder.Append(string.Format("Дисковое пространство: {0}\n", DiskCapacity));
            builder.Append(string.Format("Свободное дисковое пространство: {0}", DiskFreeSpace));
            return builder.ToString();
        }
    }
}