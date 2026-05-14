using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongBenhVien.Models
{
    public class Reception
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string ReceptionCode { get; set; } = string.Empty; // Mã tiếp đón

        public int PatientId { get; set; }

        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }

        [StringLength(100)]
        public string Department { get; set; } = string.Empty; // Khoa khám

        [StringLength(50)]
        public string Priority { get; set; } = "Thường"; // Thường, Ưu tiên, Cấp cứu

        [StringLength(20)]
        public string Status { get; set; } = "Chờ khám"; // Chờ khám, Đang khám, Đã khám, Hủy

        public int QueueNumber { get; set; } // Số thứ tự

        [StringLength(500)]
        public string Reason { get; set; } = string.Empty; // Lý do khám

        public DateTime CheckInTime { get; set; } = DateTime.Now;

        public DateTime? CheckOutTime { get; set; }
    }
}
