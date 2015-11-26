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
            builder.Append($"Время: {Time}\n");
            builder.Append($"ID: {MachineIdentifier}\n");
            builder.Append($"Загруженность процессора: {ProcessorLoad}\n");
            builder.Append($"Доступная оперативная память: {AvailablePhysicalMemory}\n");
            builder.Append($"Полная оперативная память: {TotalPhysicalMemory}\n");
            builder.Append($"Дисковое пространство: {DiskCapacity}\n");
            builder.Append($"Свободное дисковое пространство: {DiskFreeSpace}");
            return builder.ToString();
        }
    }
}