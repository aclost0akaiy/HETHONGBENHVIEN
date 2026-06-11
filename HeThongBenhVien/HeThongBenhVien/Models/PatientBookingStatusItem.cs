namespace HeThongBenhVien.Models
{
    public class PatientBookingStatusItem
    {
        public Appointment Appointment { get; set; } = null!;
        public string? DoctorSms { get; set; }
        public string? DoctorName { get; set; }
    }
}
