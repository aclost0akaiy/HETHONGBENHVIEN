using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongBenhVien.Models
{
    [Table("WorkSchedules")] // Map chính xác với tên bảng trong SQL
    public class LichLamViec
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn nhân sự")]
        public int UserId { get; set; }

        // Móc nối với bảng User để lấy Tên Bác sĩ/Nhân sự
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập thời gian")]
        [StringLength(50)]
        public string WorkTime { get; set; } // Thời gian (VD: 07:00 - 15:00)

        [Required(ErrorMessage = "Vui lòng chọn ca làm việc")]
        [StringLength(50)]
        public string ShiftName { get; set; } // Ca (VD: Ca Sáng)

        [Required(ErrorMessage = "Vui lòng chọn ngày")]
        [DataType(DataType.Date)]
        public DateTime WorkDate { get; set; } // Ngày làm việc

        public int WeekNumber { get; set; } // Tuần
        public int MonthNumber { get; set; } // Tháng
        public int YearNumber { get; set; } // Năm
    }
}