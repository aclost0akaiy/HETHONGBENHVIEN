using System.ComponentModel.DataAnnotations;

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

        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [StringLength(100)]
        public string HeadDoctor { get; set; } = string.Empty;

        public int TotalBeds { get; set; }

        public int OccupiedBeds { get; set; }

        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
