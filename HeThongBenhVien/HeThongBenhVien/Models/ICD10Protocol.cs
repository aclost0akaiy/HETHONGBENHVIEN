using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongBenhVien.Models
{
    [Table("ICD10Protocols")]
    public class ICD10Protocol
    {
        [Key]
        [Column(TypeName = "nvarchar(50)")]
        public string ICDCode { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(255)")]
        public string Diagnosis { get; set; } = string.Empty;

        public string? TreatmentPlan { get; set; }
        
        public string? LabTests { get; set; }
        
        public string? Medicines { get; set; }
    }
}
