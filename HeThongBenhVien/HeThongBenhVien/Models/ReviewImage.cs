using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongBenhVien.Models
{
    public class ReviewImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int QualityReviewId { get; set; }

        [ForeignKey("QualityReviewId")]
        public virtual QualityReview? QualityReview { get; set; }

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ImagePath { get; set; } // Đường dẫn lưu trữ ảnh đính kèm

    }
}
