namespace HeThongBenhVien.Models
{
    /// <summary>
    /// Định nghĩa tập trung tất cả trạng thái của Appointment.Status
    /// Dùng static class để tương thích với kiểu int trong DB
    /// </summary>
    public static class AppointmentStatus
    {
        public const int ChuaDen       = 0;   // Hẹn online, chưa check-in vào viện
        public const int ChoKham       = 1;   // Đã đến, xếp hàng chờ bác sĩ
        public const int DangKham      = 2;   // Bác sĩ đang tiếp đón, khám lâm sàng
        public const int ChoXetNghiem = 3;   // Bác sĩ đã chỉ định CLS/siêu âm/XN
        public const int ChoKetQua    = 4;   // Phòng XN đã tiếp nhận, BN chờ trả kết quả
        public const int ChoToaThuoc  = 5;   // Đã có KQ XN trả về, BN chờ bác sĩ kê đơn
        public const int ChoThanhToan = 6;   // Bác sĩ đã kê đơn xong, BN chờ ra quầy thu ngân
        public const int ChoLayThuoc  = 7;   // Đã đóng tiền xong, BN chờ nhà thuốc phát thuốc
        public const int HenTaiKham   = 8;   // Quy trình khám hoàn thành, có lịch tái khám
        public const int DaXacNhan    = 9;   // Hẹn online đã được bác sĩ xác nhận (trạng thái trung gian)
        public const int HoanThanh    = 10;  // Kết thúc quy trình, ra về, không cần tái khám
        public const int NhapVien     = 11;  // Bác sĩ chỉ định nhập viện điều trị nội trú

        /// <summary>Trả về nhãn tiếng Việt cho từng trạng thái</summary>
        public static string GetLabel(int status) => status switch
        {
            ChuaDen       => "Đang khám",
            ChoKham       => "Đang khám",
            DangKham      => "Đang khám",
            ChoXetNghiem  => "Chờ KQ xét nghiệm",
            ChoKetQua     => "Chờ KQ xét nghiệm",
            ChoToaThuoc   => "Đã có KQ xét nghiệm chờ lập đơn thuốc",
            ChoThanhToan  => "Chờ đơn thuốc",
            ChoLayThuoc   => "Đã có đơn thuốc",
            HenTaiKham    => "Hoàn thành khám",
            DaXacNhan     => "Hoàn thành khám",
            HoanThanh     => "Hoàn thành khám",
            NhapVien      => "Nhập viện",
            _             => "Không xác định"
        };

        /// <summary>Trả về CSS Bootstrap badge class cho từng trạng thái</summary>
        public static string GetBadgeClass(int status) => status switch
        {
            ChuaDen       => "badge-chua-den",
            ChoKham       => "badge-cho-kham",
            DangKham      => "badge-dang-kham",
            ChoXetNghiem  => "badge-cho-xet-nghiem",
            ChoKetQua     => "badge-cho-ket-qua",
            ChoToaThuoc   => "badge-cho-toa-thuoc",
            ChoThanhToan  => "badge-cho-thanh-toan",
            ChoLayThuoc   => "badge-cho-lay-thuoc",
            HenTaiKham    => "badge-hen-tai-kham",
            DaXacNhan     => "badge-da-xac-nhan",
            HoanThanh     => "badge-hoan-thanh",
            NhapVien      => "badge-nhap-vien",
            _             => "badge-chua-den"
        };

        /// <summary>Trả về icon FontAwesome cho từng trạng thái</summary>
        public static string GetIcon(int status) => status switch
        {
            ChuaDen       => "fa-clock",
            ChoKham       => "fa-hourglass-half",
            DangKham      => "fa-stethoscope",
            ChoXetNghiem  => "fa-flask",
            ChoKetQua     => "fa-microscope",
            ChoToaThuoc   => "fa-prescription",
            ChoThanhToan  => "fa-money-bill-wave",
            ChoLayThuoc   => "fa-pills",
            HenTaiKham    => "fa-calendar-check",
            DaXacNhan     => "fa-check-double",
            HoanThanh     => "fa-flag-checkered",
            NhapVien      => "fa-bed-pulse",
            _             => "fa-question-circle"
        };
    }
}
