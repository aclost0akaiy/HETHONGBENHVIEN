using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongBenhVien.Models
{
    public class HospitalFee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string FeeCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string FeeName { get; set; } = string.Empty;

        [StringLength(100)]
        public string Category { get; set; } = string.Empty; // Khám bệnh, Xét nghiệm, Phẫu thuật, Giường bệnh...

        public decimal Price { get; set; }

        public decimal InsuranceCoverage { get; set; } // % BHYT chi trả

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
