using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongBenhVien.Models
{
    public class Prescription
    {
        [Key]
        public int Id { get; set; }

        public int MedicalRecordId { get; set; }

        [ForeignKey("MedicalRecordId")]
        public MedicalRecord? MedicalRecord { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Đã kê đơn";

        public ICollection<PrescriptionDetail> PrescriptionDetails { get; set; } = new List<PrescriptionDetail>();
    }
}
