using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongBenhVien.Models
{
    public class InsuranceCard
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string CardNumber { get; set; } = string.Empty; // Số thẻ BHYT

        public int PatientId { get; set; }

        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }

        [StringLength(200)]
        public string RegisteredHospital { get; set; } = string.Empty; // Nơi đăng ký KCB ban đầu

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int CoveragePercent { get; set; } = 80; // % chi trả

        [StringLength(20)]
        public string Status { get; set; } = "Còn hiệu lực"; // Còn hiệu lực, Hết hạn

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
