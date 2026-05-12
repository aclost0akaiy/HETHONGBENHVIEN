using System;

namespace HeThongBenhVien.Models
{
    public class MedicalEquipment
    {
        public int Id { get; set; }
        public string EquipmentCode { get; set; }
        public string EquipmentName { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; } // e.g. Hoạt động, Bảo trì, Hỏng
        public DateTime LastMaintenanceDate { get; set; }
    }
}
