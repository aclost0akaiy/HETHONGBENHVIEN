using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongBenhVien.Models
{
    public class MedicalRecord
    {
        [Key]
        public int Id { get; set; }

        public int AppointmentId { get; set; }

        [ForeignKey("AppointmentId")]
        public Appointment? Appointment { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập triệu chứng")]
        [Display(Name = "Triệu chứng lâm sàng")]
        public string Symptoms { get; set; } = string.Empty;


        [Required(ErrorMessage = "Vui lòng nhập chẩn đoán")]
        [Display(Name = "Chẩn đoán bệnh")]
        public string Diagnosis { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập hướng điều trị")]
        [Display(Name = "Hướng điều trị / Kê đơn")]
        public string TreatmentPlan { get; set; } = string.Empty;

        [Display(Name = "Ghi chú thêm")]
        public string? Notes { get; set; }

        public int? DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }
        
        public int? BedNumber { get; set; }

        public DateTime? AdmissionDate { get; set; }
        public DateTime? DischargeDate { get; set; }
        public decimal RoomFee { get; set; } = 65000;

        public int? SurgeonId { get; set; }
        public int? SurgeryFeeId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsLocked { get; set; } = false;
        
        public string? DigitalSignature { get; set; }
    }
}
