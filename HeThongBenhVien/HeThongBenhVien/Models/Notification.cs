using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongBenhVien.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; }

        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }

        [ForeignKey("DoctorId")]
        public User? Doctor { get; set; }
    }
}
