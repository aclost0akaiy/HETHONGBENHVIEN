using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // 1. THÊM DÒNG NÀY Ở ĐẦU FILE

namespace HeThongBenhVien.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string DepartmentCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string DepartmentName { get; set; } = string.Empty;

        // 2. THÊM ATTRIBUTE NÀY ĐỂ CHẶN ENTITY FRAMEWORK TRUY VẤN XUỐNG DB
        [NotMapped]
        public string Name
        {
            get => DepartmentName;
            set => DepartmentName = value;
        }

        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string HeadDoctor { get; set; } = string.Empty;

        public int TotalBeds { get; set; } = 50;

        public int OccupiedBeds { get; set; }

        [Required]
        [StringLength(10)]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải bắt đầu bằng số 0 và có đúng 10 chữ số")]
        public string Phone { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}