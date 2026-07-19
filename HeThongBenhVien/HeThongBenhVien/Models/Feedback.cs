using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongBenhVien.Models
{
    public class Feedback
    {
        [Key]
        public int Id { get; set; }

        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }

        /// <summary>Đánh giá tổng quát (1-5 sao)</summary>
        public int RatingOverall { get; set; }

        /// <summary>Thái độ phục vụ (1-5 sao)</summary>
        public int ThaiDo { get; set; }

        /// <summary>Vệ sinh sạch sẽ (1-5 sao)</summary>
        public int VeSinh { get; set; }

        /// <summary>Chuyên môn bác sĩ (Kém / Trung bình / Tốt / Xuất sắc)</summary>
        [StringLength(50)]
        public string? ChuyenMon { get; set; }

        /// <summary>Cơ sở vật chất (Kém / Trung bình / Tốt)</summary>
        [StringLength(50)]
        public string? CSVC { get; set; }

        /// <summary>Thời gian chờ đợi (< 15p / 15-30p / 30-60p / 1h-2h / > 2h)</summary>
        [StringLength(50)]
        public string? ThoiGianCho { get; set; }

        /// <summary>Nội dung nhận xét chi tiết</summary>
        [StringLength(2000)]
        public string? Content { get; set; }

        /// <summary>Đường dẫn hình ảnh đính kèm (phân cách bằng ;)</summary>
        [StringLength(2000)]
        public string? ImageUrl { get; set; }

        /// <summary>Trạng thái: Pending / Responded / Closed</summary>
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        /// <summary>Cho phép bệnh viện liên hệ phản hồi</summary>
        public bool AllowContact { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
