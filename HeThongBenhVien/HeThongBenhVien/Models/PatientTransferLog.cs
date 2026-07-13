using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongBenhVien.Models
{
    [Table("PatientTransferLogs")]
    public class PatientTransferLog
    {
        [Key]
        public int Id { get; set; }

        public int AppointmentId { get; set; }

        public int FromDoctorId { get; set; }

        public int ToDoctorId { get; set; }

        public string? Situation { get; set; }

        public string? Background { get; set; }

        public string? Assessment { get; set; }

        public string? Recommendation { get; set; }

        public bool IsEmergencyConsultation { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("AppointmentId")]
        public virtual Appointment? Appointment { get; set; }

        [ForeignKey("FromDoctorId")]
        public virtual User? FromDoctor { get; set; }

        [ForeignKey("ToDoctorId")]
        public virtual User? ToDoctor { get; set; }
    }
}
