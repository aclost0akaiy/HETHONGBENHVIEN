using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongBenhVien.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        public int PatientId { get; set; }

        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }

        [Required]
        [StringLength(200)]
        public string Reason { get; set; } = string.Empty;

        public DateTime AppointmentTime { get; set; }

        // Trạng thái: 0 - Chưa đến, 1 - Đang chờ, 2 - Đang khám, 3 - Có KQ XN, 4 - Đã khám xong
        public int Status { get; set; } 
    }
}
