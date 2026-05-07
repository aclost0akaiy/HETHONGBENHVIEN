using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongBenhVien.Models
{
    public class LabTest
    {
        [Key]
        public int Id { get; set; }

        public int MedicalRecordId { get; set; }

        [ForeignKey("MedicalRecordId")]
        public MedicalRecord? MedicalRecord { get; set; }

        public string TestName { get; set; } = string.Empty;

        public string Status { get; set; } = "Chờ xét nghiệm";

        public string? Result { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? CompletedAt { get; set; }
    }
}
