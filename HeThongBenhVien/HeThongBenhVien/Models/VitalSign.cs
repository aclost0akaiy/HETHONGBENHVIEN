using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongBenhVien.Models
{
    public class VitalSign
    {
        [Key]
        public int Id { get; set; }

        public int AppointmentId { get; set; }

        [ForeignKey("AppointmentId")]
        public virtual Appointment? Appointment { get; set; }

        public string? Pulse { get; set; } // Mạch (l/p)
        public string? Temperature { get; set; } // Nhiệt độ (°C)
        public string? BloodPressure { get; set; } // Huyết áp (mmHg)
        public string? SpO2 { get; set; } // SpO2 (%)
        public string? NurseName { get; set; } // Điều dưỡng đo

        public DateTime RecordedAt { get; set; } = DateTime.Now;
    }
}
