using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeThongBenhVien.Migrations
{
    /// <inheritdoc />
    public partial class FinalSchemaFixFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Sửa bảng Users: Ép kiểu PatientCode về NULL để không lỗi khi Seed dữ liệu
            migrationBuilder.Sql(@"IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Users]') AND name = N'PatientCode') 
                ALTER TABLE [Users] ALTER COLUMN [PatientCode] nvarchar(20) NULL;");
            
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Users]') AND name = N'DepartmentId') 
                ALTER TABLE [Users] ADD [DepartmentId] int NULL;");

            // 2. Sửa bảng Appointments
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Appointments]') AND name = N'DoctorId') 
                ALTER TABLE [Appointments] ADD [DoctorId] int NULL;");

            // 3. Sửa bảng Notifications
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Notifications]') AND name = N'Type') 
                ALTER TABLE [Notifications] ADD [Type] int NOT NULL DEFAULT 0;");
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Notifications]') AND name = N'IsForPatient') 
                ALTER TABLE [Notifications] ADD [IsForPatient] bit NOT NULL DEFAULT 0;");

            // 4. Tạo bảng DoctorDepartments nếu chưa có
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[DoctorDepartments]') AND type in (N'U'))
            CREATE TABLE [DoctorDepartments] (
                [Id] int NOT NULL IDENTITY,
                [DoctorId] int NOT NULL,
                [DepartmentId] int NOT NULL,
                CONSTRAINT [PK_DoctorDepartments] PRIMARY KEY ([Id]),
                CONSTRAINT [FK_DoctorDepartments_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE CASCADE,
                CONSTRAINT [FK_DoctorDepartments_Users_DoctorId] FOREIGN KEY ([DoctorId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
            );");

            // 5. Thêm Foreign Keys
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = N'FK_Users_Departments_DepartmentId')
                ALTER TABLE [Users] ADD CONSTRAINT [FK_Users_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]);");

            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = N'FK_Appointments_Users_DoctorId')
                ALTER TABLE [Appointments] ADD CONSTRAINT [FK_Appointments_Users_DoctorId] FOREIGN KEY ([DoctorId]) REFERENCES [Users] ([Id]);");
        }
    }
}
