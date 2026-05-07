using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongBenhVien.Models
{
    public class PrescriptionDetail
    {
        [Key]
        public int Id { get; set; }

        public int PrescriptionId { get; set; }

        [ForeignKey("PrescriptionId")]
        public Prescription? Prescription { get; set; }

        public string MedicineName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public string Unit { get; set; } = string.Empty;

        public string DosageInstruction { get; set; } = string.Empty;
    }
}
