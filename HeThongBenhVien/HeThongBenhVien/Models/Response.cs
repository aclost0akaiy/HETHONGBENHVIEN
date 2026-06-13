using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongBenhVien.Models
{
    public class Response
    {
        [Key]
        public int Id { get; set; }

        public int FeedbackId { get; set; }

        [ForeignKey("FeedbackId")]
        public virtual Feedback? Feedback { get; set; }

        /// <summary>Người gửi: Admin hoặc User</summary>
        [Required]
        [StringLength(20)]
        public string Sender { get; set; } = "User"; // "Admin" or "User"

        /// <summary>Nội dung phản hồi</summary>
        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
