using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server
{
    public class Performance
    {
        [Key, Column(Order = 1), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long PerformanceId { get; set; }

        [Key, Column(Order = 2)]
        public string MachineIdentifier { get; set; }

        public DateTime Time { get; set; }

        [Column(TypeName = "smallint")]
        public short ProcessorLoad { get; set; }

        [Column(TypeName = "bigint")]
        public long AvailablePhysicalMemory { get; set; }

        [Column(TypeName = "bigint")]
        public long TotalPhysicalMemory { get; set; }

        [Column(TypeName = "bigint")]
        public long DiskCapacity{ get; set; }

        [Column(TypeName = "bigint")]
        public long DiskFreeSpace { get; set; }
    }
}