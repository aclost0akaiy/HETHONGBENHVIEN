using System;
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

        // Work Schedule properties
        public List<LichLamViec> WorkSchedules { get; set; } = new List<LichLamViec>();
        public string CurrentUsername { get; set; } = string.Empty;
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public List<User> AllDoctors { get; set; } = new List<User>();
    }
}
