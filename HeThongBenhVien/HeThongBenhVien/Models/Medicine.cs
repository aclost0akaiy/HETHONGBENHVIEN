using System;
using System.ComponentModel.DataAnnotations;

namespace HeThongBenhVien.Models
{
    public class Medicine
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        [StringLength(50)]
        public string Unit { get; set; } = string.Empty; // Viên, Hộp, Chai, Ống...

        [StringLength(100)]
        public string Category { get; set; } = string.Empty; // Kháng sinh, Giảm đau, Tim mạch...

        public int StockQuantity { get; set; } = 0;

        public int MinStock { get; set; } = 10; // Số lượng tồn kho tối thiểu

        [StringLength(200)]
        public string Manufacturer { get; set; } = string.Empty; // Nhà sản xuất

        public DateTime? ExpiryDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}