using System.Collections.Generic;

namespace HeThongBenhVien.Models
{
    public class DoctorDashboardViewModel
    {
        // === Các field cũ (giữ nguyên) ===
        public int TodayPatientsCount { get; set; }
        public int CompletedPatientsCount { get; set; }
        public int WaitingResultsCount { get; set; }
        public int EmergencyCount { get; set; }

        public List<Appointment> UpcomingAppointments { get; set; } = new List<Appointment>();
        public List<Appointment> PendingOnlineAppointments { get; set; } = new List<Appointment>();
        public List<Appointment> ConfirmedAppointments { get; set; } = new List<Appointment>();

        // Weekly work schedule entries
        public List<LichLamViec> WorkSchedules { get; set; } = new List<LichLamViec>();

        public int? CurrentUserId { get; set; }
        public int CurrentMonth { get; set; }
        public int CurrentYear { get; set; }
        public string SearchString { get; set; } = string.Empty;
        public List<Patient> Patients { get; set; } = new List<Patient>();

        // === THÊM MỚI: Dữ liệu cảnh báo ưu tiên ===
        /// <summary>Số BN ở trạng thái Chờ toa thuốc (Status=5) - cần bác sĩ kê đơn ngay</summary>
        public int WaitingPrescriptionCount { get; set; }

        /// <summary>Số BN chờ khám > 30 phút</summary>
        public int OverdueWaitingCount { get; set; }

        /// <summary>Danh sách lịch mổ hôm nay</summary>
        public List<Surgery> TodaySurgeries { get; set; } = new List<Surgery>();

        /// <summary>Danh sách BN hẹn tái khám hôm nay</summary>
        public List<Appointment> TodayFollowUps { get; set; } = new List<Appointment>();

        /// <summary>Danh sách cảnh báo cận lâm sàng từ DB</summary>
        public List<LabTest> LabAlerts { get; set; } = new List<LabTest>();

        /// <summary>Danh sách cảnh báo sinh hiệu từ DB</summary>
        public List<VitalSign> VitalAlerts { get; set; } = new List<VitalSign>();
    }
}
