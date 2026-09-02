using System;
using System.Collections.Generic;

namespace HeThongBenhVien.Models
{
    public class ActivityReportViewModel
    {
        public ActivitySummaryDto Summary { get; set; } = new();
        public List<IcdDiseaseCategoryDto> DiseaseDistribution { get; set; } = new();
        public List<DepartmentPerformanceDto> DepartmentPerformance { get; set; } = new();
    }

    public class ActivitySummaryDto
    {
        public int TotalVisits { get; set; } // Tổng lượt tiếp nhận
        public int PreviousPeriodVisits { get; set; }
        public double VisitGrowthPercent { get; set; } // % tăng/giảm so với kỳ trước
        
        public int CompletedRecords { get; set; } // Hồ sơ hoàn thành
        public double CompletionRatePercent { get; set; } // Tỷ lệ xử lý (e.g. 95.2%)
        
        public int InpatientCount { get; set; } // Số ca nhập viện nội trú
        public double InpatientRatePercent { get; set; } // Tỷ lệ nhập viện (e.g. 14.5%)
        
        public int AvgProcessingTimeMinutes { get; set; } // Thời gian xử lý trung bình (phút/lượt)
    }

    public class IcdDiseaseCategoryDto
    {
        public string IcdCode { get; set; } = string.Empty; // e.g. "I10"
        public string DiseaseName { get; set; } = string.Empty; // e.g. "Tăng huyết áp vô căn"
        public string FullLabel => $"{IcdCode} - {DiseaseName}";
        public int CaseCount { get; set; }
        public double Percentage { get; set; }
        public string ColorHex { get; set; } = "#3b82f6";
    }

    public class DepartmentPerformanceDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int VisitCount { get; set; } // Số lượt khám
        public decimal TotalRevenue { get; set; } // Doanh thu / Chi phí điều trị
    }
}
