using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongBenhVien.Models
{
    public class Surgery
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string SurgeryCode { get; set; } = string.Empty;

        public int PatientId { get; set; }

        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }

        [Required]
        [StringLength(200)]
        public string SurgeryName { get; set; } = string.Empty; // Tên phẫu thuật

        [StringLength(100)]
        public string SurgeryType { get; set; } = string.Empty; // Loại: Đại phẫu, Trung phẫu, Tiểu phẫu

        [StringLength(100)]
        public string Surgeon { get; set; } = string.Empty; // Bác sĩ phẫu thuật chính

        [StringLength(200)]
        public string AssistantTeam { get; set; } = string.Empty; // Ekip phụ mổ

        [StringLength(100)]
        public string Anesthesia { get; set; } = string.Empty; // Phương pháp gây mê

        [StringLength(50)]
        public string OperatingRoom { get; set; } = string.Empty; // Phòng mổ

        public DateTime ScheduledDate { get; set; }

        public int DurationMinutes { get; set; } // Thời gian dự kiến (phút)

        [StringLength(20)]
        public string Status { get; set; } = "Lên lịch"; // Lên lịch, Đang mổ, Hoàn thành, Hủy

        [StringLength(1000)]
        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
