using Microsoft.EntityFrameworkCore;
using HeThongBenhVien.Models;

namespace HeThongBenhVien.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<PrescriptionDetail> PrescriptionDetails { get; set; }
        public DbSet<LabTest> LabTests { get; set; }
        public DbSet<MedicineUnit> MedicineUnits { get; set; }
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<MedicalService> MedicalServices { get; set; }
        public DbSet<MedicalEquipment> MedicalEquipments { get; set; }
        public DbSet<VitalSign> VitalSigns { get; set; }

        // Đã thêm khai báo bảng Lịch Làm Việc vào đây:
        public DbSet<LichLamViec> LichLamViecs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}