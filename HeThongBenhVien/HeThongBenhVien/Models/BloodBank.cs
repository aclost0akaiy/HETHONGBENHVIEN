using System;
using System.ComponentModel.DataAnnotations;

namespace HeThongBenhVien.Models
{
    public class BloodBank
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string BagCode { get; set; } = string.Empty; // Mã túi máu

        [Required]
        [StringLength(5)]
        public string BloodType { get; set; } = string.Empty; // A+, A-, B+, B-, AB+, AB-, O+, O-

        [StringLength(20)]
        public string Component { get; set; } = "Máu toàn phần"; // Máu toàn phần, Hồng cầu, Tiểu cầu, Huyết tương

        public int VolumeMl { get; set; } // Thể tích (ml)

        [StringLength(100)]
        public string DonorName { get; set; } = string.Empty; // Tên người hiến

        [StringLength(20)]
        public string DonorPhone { get; set; } = string.Empty;

        public DateTime CollectionDate { get; set; } = DateTime.Now;

        public DateTime ExpiryDate { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Khả dụng"; // Khả dụng, Đã sử dụng, Hết hạn, Hủy

        [StringLength(200)]
        public string Notes { get; set; } = string.Empty;
    }
}
