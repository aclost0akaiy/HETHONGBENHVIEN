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

        [Required(ErrorMessage = "Vui lòng nhập sinh hiệu")]
        [Display(Name = "Sinh hiệu (Huyết áp, Nhịp tim, Nhiệt độ)")]
        public string Vitals { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập chẩn đoán")]
        [Display(Name = "Chẩn đoán bệnh")]
        public string Diagnosis { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập hướng điều trị")]
        [Display(Name = "Hướng điều trị / Kê đơn")]
        public string TreatmentPlan { get; set; } = string.Empty;

        [Display(Name = "Ghi chú thêm")]
        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
