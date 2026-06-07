using System.Collections.Generic;

namespace HeThongBenhVien.Models
{
    public class DoctorDashboardViewModel
    {
        public int TodayPatientsCount { get; set; }
        public int CompletedPatientsCount { get; set; }
        public int WaitingResultsCount { get; set; }
        public int EmergencyCount { get; set; }
        
        public List<Appointment> UpcomingAppointments { get; set; } = new List<Appointment>();
        // Weekly work schedule entries (for the current week displayed on dashboard)
        public List<LichLamViec> WorkSchedules { get; set; } = new List<LichLamViec>();

        // The currently logged in user id (if available) to highlight their name
        public int? CurrentUserId { get; set; }
        public int CurrentMonth { get; set; }
        public int CurrentYear { get; set; }
        public string SearchString { get; set; } = string.Empty;
        public List<Patient> Patients { get; set; } = new List<Patient>();
    }
}
