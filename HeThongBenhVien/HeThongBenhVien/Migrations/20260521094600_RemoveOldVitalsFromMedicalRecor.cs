using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeThongBenhVien.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOldVitalsFromMedicalRecor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Vitals",
                table: "MedicalRecords");

            migrationBuilder.AddColumn<int>(
                name: "BedNumber",
                table: "MedicalRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "MedicalRecords",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_DepartmentId",
                table: "MedicalRecords",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalRecords_Departments_DepartmentId",
                table: "MedicalRecords",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalRecords_Departments_DepartmentId",
                table: "MedicalRecords");

            migrationBuilder.DropIndex(
                name: "IX_MedicalRecords_DepartmentId",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "BedNumber",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "MedicalRecords");

            migrationBuilder.AddColumn<string>(
                name: "Vitals",
                table: "MedicalRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
