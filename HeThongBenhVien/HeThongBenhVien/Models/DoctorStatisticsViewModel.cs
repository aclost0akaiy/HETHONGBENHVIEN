using System.Collections.Generic;

namespace HeThongBenhVien.Models
{
    public class MonthDiseaseStat
    {
        public string Diagnosis { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percent { get; set; }
    }

    public class DoctorStatisticsViewModel
    {
        public int SelectedMonth { get; set; }
        public int SelectedYear { get; set; }
        public int TotalPatientsExamined { get; set; }
        public int TotalVisits { get; set; }
        public decimal TotalRevenue { get; set; }
        public int DuplicateDiagnosisCount { get; set; }
        public decimal DuplicateDiagnosisRate { get; set; }
        public List<MonthDiseaseStat> DiseaseStats { get; set; } = new List<MonthDiseaseStat>();
    }
}
