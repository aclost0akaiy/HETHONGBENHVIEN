using System.ComponentModel.DataAnnotations;

namespace HeThongBenhVien.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = string.Empty; // "Admin" hoặc "Doctor"

        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;
    }
}
