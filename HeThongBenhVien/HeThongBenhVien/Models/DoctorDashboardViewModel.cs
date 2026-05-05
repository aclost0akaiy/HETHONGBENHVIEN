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
    }
}
