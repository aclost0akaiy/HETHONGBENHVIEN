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
        public string Name { get; set; } = string.Empty; // Biệt dược / Tên thuốc

        [StringLength(200)]
        public string? ActiveIngredient { get; set; } = string.Empty; // Hoạt chất (e.g., Paracetamol, Atropin...)

        [StringLength(100)]
        public string? Dosage { get; set; } = string.Empty; // Hàm lượng (e.g., 500mg, 100mg/2ml)

        [StringLength(100)]
        public string? DosageForm { get; set; } = string.Empty; // Dạng bào chế (Viên nén, Dung dịch tiêm, Ống...)

        [StringLength(50)]
        public string? BatchNumber { get; set; } = string.Empty; // Số lô (e.g., L240101, LO-2026A)

        public decimal Price { get; set; } // Giá bán

        public decimal PurchasePrice { get; set; } = 0; // Giá mua

        [StringLength(50)]
        public string? Unit { get; set; } = string.Empty; // Viên, Hộp, Chai, Ống...

        [StringLength(100)]
        public string? Category { get; set; } = string.Empty; // Thuốc tiêm, Thuốc thường, Kháng sinh... (KHÔNG có Vật tư y tế tiêu hao)

        public int StockQuantity { get; set; } = 0;

        public int MinStock { get; set; } = 100; // Số lượng tồn kho tối thiểu / Cảnh báo tồn an toàn (Mặc định 100)

        [StringLength(200)]
        public string? Manufacturer { get; set; } = string.Empty; // Nhà sản xuất / Hãng sản xuất

        public DateTime? ExpiryDate { get; set; } // Hạn sử dụng

        [StringLength(300)]
        public string? Usage { get; set; } = string.Empty; // Cách dùng (e.g. (Uống), Ngày 02 lần, mỗi lần 01 viên)

        public bool IsActive { get; set; } = true;
    }
}