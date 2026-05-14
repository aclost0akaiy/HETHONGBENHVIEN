using System;
using System.ComponentModel.DataAnnotations;

namespace HeThongBenhVien.Models
{
    public class QualityReview
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty; // Khoa được đánh giá

        [Required]
        [StringLength(100)]
        public string ReviewerName { get; set; } = string.Empty; // Người đánh giá

        public int ServiceScore { get; set; } // Điểm dịch vụ (1-10)

        public int CleanlinessScore { get; set; } // Điểm vệ sinh (1-10)

        public int StaffScore { get; set; } // Điểm nhân viên (1-10)

        public int FacilityScore { get; set; } // Điểm cơ sở vật chất (1-10)

        public int WaitTimeScore { get; set; } // Điểm thời gian chờ (1-10)

        [StringLength(1000)]
        public string Comment { get; set; } = string.Empty;

        [StringLength(20)]
        public string Status { get; set; } = "Chờ xử lý"; // Chờ xử lý, Đã xử lý, Đã phản hồi

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
