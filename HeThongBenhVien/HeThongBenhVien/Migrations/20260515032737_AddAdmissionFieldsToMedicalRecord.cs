using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeThongBenhVien.Migrations
{
    /// <inheritdoc />
    public partial class AddAdmissionFieldsToMedicalRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AdmissionDate",
                table: "MedicalRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DischargeDate",
                table: "MedicalRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RoomFee",
                table: "MedicalRecords",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdmissionDate",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "DischargeDate",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "RoomFee",
                table: "MedicalRecords");
        }
    }
}
