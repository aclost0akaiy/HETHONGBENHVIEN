using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongBenhVien.Models
{
    public class DiagnosticImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string RequestCode { get; set; } = string.Empty;

        public int PatientId { get; set; }

        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }

        [Required]
        [StringLength(100)]
        public string ImageType { get; set; } = string.Empty; // X-Quang, CT Scanner, MRI, Siêu âm, Nội soi

        [StringLength(200)]
        public string BodyPart { get; set; } = string.Empty; // Vùng chụp

        [StringLength(100)]
        public string RequestedBy { get; set; } = string.Empty; // Bác sĩ chỉ định

        [StringLength(1000)]
        public string Result { get; set; } = string.Empty;

        [StringLength(500)]
        public string Conclusion { get; set; } = string.Empty;

        [StringLength(20)]
        public string Status { get; set; } = "Chờ chụp"; // Chờ chụp, Đang chụp, Có kết quả

        public DateTime RequestDate { get; set; } = DateTime.Now;

        public DateTime? CompletedDate { get; set; }
    }
}
