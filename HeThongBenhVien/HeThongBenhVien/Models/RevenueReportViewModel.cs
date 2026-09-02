using System;
using System.Collections.Generic;

namespace HeThongBenhVien.Models
{
    public class RevenueReportViewModel
    {
        public RevenueSummaryDto Summary { get; set; } = new();
        public List<TimeChartPointDto> TimeSeries { get; set; } = new();
        public List<CategoryRevenueDto> ServiceCategories { get; set; } = new();
        public OutpatientInpatientDto PatientTypes { get; set; } = new();
        public List<DepartmentRevenueDto> DepartmentRevenues { get; set; } = new();
        public List<PaymentMethodRevenueDto> PaymentMethods { get; set; } = new();
        public List<PaymentStatusRevenueDto> PaymentStatuses { get; set; } = new();
        public List<RevenueTransactionDto> RecentTransactions { get; set; } = new();
    }

    public class RevenueSummaryDto
    {
        public decimal TotalRevenue { get; set; } // BN Thanh Toán tổng (Đã thanh toán)
        public decimal TotalCost { get; set; } // Tổng Chi Phí (Phần BHYT + BN)
        public decimal TotalInsurancePaid { get; set; } // Tổng phần BHYT chi trả
        public decimal TotalPatientPaid { get; set; } // Tổng phần BN cùng chi trả
        public decimal PreviousPeriodRevenue { get; set; }
        public double GrowthRatePercent { get; set; } // % tăng/giảm so với kỳ trước
        public decimal GrowthAmount { get; set; } // Số tiền tăng/giảm
        public string GrowthDirection { get; set; } = "neutral"; // "up", "down", "neutral"
        
        public int TotalVisits { get; set; }
        public int PreviousPeriodVisits { get; set; }
        public double VisitGrowthPercent { get; set; }
        
        public decimal AvgRevenuePerVisit { get; set; }
        
        public decimal OutpatientRevenue { get; set; }
        public int OutpatientCount { get; set; }
        public double OutpatientPercentage { get; set; }
        
        public decimal InpatientRevenue { get; set; }
        public int InpatientCount { get; set; }
        public double InpatientPercentage { get; set; }

        public decimal PaidAmount { get; set; }
        public decimal UnpaidAmount { get; set; }
        public decimal RefundedAmount { get; set; }

        public int TotalServiceUsageCount { get; set; }
    }

    public class TimeChartPointDto
    {
        public string Label { get; set; } = string.Empty;
        public string TimeKey { get; set; } = string.Empty; // e.g. "2026-08-22"
        public decimal Revenue { get; set; }
        public int VisitCount { get; set; }
        public int ServiceCount { get; set; }
    }

    public class CategoryRevenueDto
    {
        public string CategoryName { get; set; } = string.Empty; // Khám bệnh, CLS, Xét nghiệm, Thuốc, Giường nội trú...
        public decimal Revenue { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
        public string ColorHex { get; set; } = "#6366f1";
    }

    public class OutpatientInpatientDto
    {
        public decimal OutpatientRevenue { get; set; }
        public int OutpatientCount { get; set; }
        public double OutpatientPercentage { get; set; }

        public decimal InpatientRevenue { get; set; }
        public int InpatientCount { get; set; }
        public double InpatientPercentage { get; set; }
    }

    public class DepartmentRevenueDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int VisitCount { get; set; }
        public int BedOccupancy { get; set; }
        public double Percentage { get; set; }
    }

    public class PaymentMethodRevenueDto
    {
        public string MethodName { get; set; } = string.Empty; // Tiền mặt, Chuyển khoản QR, Thẻ POS
        public decimal Revenue { get; set; }
        public int TransactionCount { get; set; }
        public double Percentage { get; set; }
        public string IconClass { get; set; } = "fa-money-bill-wave";
    }

    public class PaymentStatusRevenueDto
    {
        public string StatusName { get; set; } = string.Empty; // Đã thanh toán, Tạm thu, Chờ thanh toán, Đã hoàn tiền
        public string StatusCode { get; set; } = string.Empty; // paid, pending, unpaid, refunded
        public decimal Amount { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
        public string BadgeClass { get; set; } = "bg-success";
    }

    public class RevenueTransactionDto
    {
        public string RecordCode { get; set; } = string.Empty; // Mã Hóa Đơn
        public string PatientCode { get; set; } = string.Empty; // Mã Bệnh Nhân
        public string PatientName { get; set; } = string.Empty; // Họ Tên Bệnh Nhân
        public string ServiceType { get; set; } = string.Empty; // Dịch vụ chính
        public string ServicesSummary { get; set; } = string.Empty; // Tổng Dịch Vụ
        public string DepartmentName { get; set; } = string.Empty; // Khoa / Phòng
        public int DepartmentId { get; set; }
        public string PatientType { get; set; } = string.Empty; // Ngoại trú / Nội trú
        public string PaymentMethod { get; set; } = string.Empty; // Tiền mặt, Chuyển khoản QR, Thẻ POS
        public string Status { get; set; } = string.Empty; // Đã thanh toán, Tạm thu, Chờ thanh toán, Đã hoàn tiền
        public string StatusBadge { get; set; } = "bg-success";
        public decimal TotalCost { get; set; } // Tổng Chi Phí (VNĐ)
        public decimal InsurancePaid { get; set; } // BHYT Chi Trả (VNĐ)
        public decimal PatientPaid { get; set; } // BN Thanh Toán (VNĐ)
        public decimal Amount { get; set; } // Legacy field = PatientPaid for backward compatibility
        public DateTime TransactionDate { get; set; } // Thời Gian
    }
}

