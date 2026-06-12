using System;
using System.ComponentModel.DataAnnotations;

namespace HeThongBenhVien.Models
{
    public class QualityReview
    {
        [Key]
        public int Id { get; set; }

        public int? UserId { get; set; } // ID người đánh giá (đăng nhập)

        public int? DepartmentId { get; set; } // ID khoa được đánh giá


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

        public int? OverallScore { get; set; } // Điểm đánh giá tổng quát (0-10)

        [StringLength(20)]
        public string? ReviewerPhone { get; set; } // Số điện thoại người đánh giá

        [StringLength(1000)]
        public string? Comment { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Chưa phản hồi"; // Chưa phản hồi, Đã phản hồi

        public bool IsAnonymous { get; set; } = false; // Đăng ẩn danh

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [StringLength(1000)]
        public string? ReplyComment { get; set; } // Nội dung phản hồi

        [StringLength(100)]
        public string? RepliedBy { get; set; } // Người phản hồi

        public DateTime? RepliedAt { get; set; } // Thời gian phản hồi

        [StringLength(500)]
        public string? AttachmentPath { get; set; } // Ảnh/File đính kèm từ người đánh giá

        [StringLength(500)]
        public string? ResponseAttachmentPath { get; set; } // Ảnh/File đính kèm phản hồi

        [StringLength(200)]
        public string? RatingReason { get; set; } // Lý do đánh giá

        public DateTime? VisitDate { get; set; } // Thời gian khám
    }
}
