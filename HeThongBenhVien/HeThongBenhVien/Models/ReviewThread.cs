using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongBenhVien.Models
{
    public class ReviewThread
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int QualityReviewId { get; set; }

        [ForeignKey("QualityReviewId")]
        public virtual QualityReview? QualityReview { get; set; }

        [Required]
        [StringLength(50)]
        public string SenderType { get; set; } = "Patient"; // "Patient" or "Admin"

        [Required]
        public string MessageContent { get; set; } = string.Empty;

        [Required]
        public int IsAdminReply { get; set; } = 0; // 0 for Patient, 1 for Admin

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
