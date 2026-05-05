using System;
using System.ComponentModel.DataAnnotations;

namespace HeThongBenhVien.Models
{
    public class Patient
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        public string Gender { get; set; } = "Nam";

        public int Age { get; set; }

        [Required]
        [StringLength(20)]
        public string PatientCode { get; set; } = string.Empty;
    }
}
