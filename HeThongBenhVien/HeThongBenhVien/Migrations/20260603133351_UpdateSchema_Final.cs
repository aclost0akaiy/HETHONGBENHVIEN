using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeThongBenhVien.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchema_Final : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SDT",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Users]') AND name = N'PatientCode') 
                ALTER TABLE [Users] ADD [PatientCode] nvarchar(20) NULL;");

            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Patients]') AND name = N'Allergies') 
                ALTER TABLE [Patients] ADD [Allergies] nvarchar(max) NULL;");

            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Patients]') AND name = N'CCCD') 
                ALTER TABLE [Patients] ADD [CCCD] nvarchar(20) NULL;");

            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Patients]') AND name = N'FaceData') 
                ALTER TABLE [Patients] ADD [FaceData] nvarchar(max) NULL;");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "MedicalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[MedicalRecords]') AND name = N'DigitalSignature') 
                ALTER TABLE [MedicalRecords] ADD [DigitalSignature] nvarchar(max) NULL;");

            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[MedicalRecords]') AND name = N'IsLocked') 
                ALTER TABLE [MedicalRecords] ADD [IsLocked] bit NOT NULL DEFAULT CAST(0 AS bit);");

            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[LabTests]') AND name = N'ImageUrl') 
                ALTER TABLE [LabTests] ADD [ImageUrl] nvarchar(max) NULL;");

            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ICD10Protocols]') AND type in (N'U'))
            CREATE TABLE [ICD10Protocols] (
                [ICDCode] nvarchar(50) NOT NULL,
                [Diagnosis] nvarchar(255) NOT NULL,
                [TreatmentPlan] nvarchar(max) NULL,
                [LabTests] nvarchar(max) NULL,
                [Medicines] nvarchar(max) NULL,
                CONSTRAINT [PK_ICD10Protocols] PRIMARY KEY ([ICDCode])
            );");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ICD10Protocols");

            migrationBuilder.DropColumn(
                name: "PatientCode",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Allergies",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "CCCD",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "FaceData",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "DigitalSignature",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "IsLocked",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "LabTests");

            migrationBuilder.AlterColumn<string>(
                name: "SDT",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "MedicalRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
