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
    }
}