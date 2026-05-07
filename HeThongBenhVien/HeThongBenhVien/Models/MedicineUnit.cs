using System.ComponentModel.DataAnnotations;

namespace HeThongBenhVien.Models
{
    public class MedicineUnit
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string UnitName { get; set; } = string.Empty;
        
        public decimal DefaultPrice { get; set; }
    }
}
