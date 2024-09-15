using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ConnectMyDoc_Data_Layer.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PatientAddresses",
                columns: table => new
                {
                    PatientAddressId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StreetAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ZipCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientAddresses", x => x.PatientAddressId);
                });

            migrationBuilder.CreateTable(
                name: "PatientGuardians",
                columns: table => new
                {
                    PatientGuardianId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientGuardianName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PatientGuardianPhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PatientGuardianRelationship = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientGuardians", x => x.PatientGuardianId);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    PatientId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    Dob = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreferredStartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PreferredEndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<int>(type: "int", nullable: false),
                    PreferredClinicId = table.Column<int>(type: "int", nullable: false),
                    PreferredDoctorId = table.Column<int>(type: "int", nullable: false),
                    Image = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    PatientAddressId = table.Column<int>(type: "int", nullable: false),
                    PatientGuardianId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.PatientId);
                    table.ForeignKey(
                        name: "FK_Patients_PatientAddresses_PatientAddressId",
                        column: x => x.PatientAddressId,
                        principalTable: "PatientAddresses",
                        principalColumn: "PatientAddressId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Patients_PatientGuardians_PatientGuardianId",
                        column: x => x.PatientGuardianId,
                        principalTable: "PatientGuardians",
                        principalColumn: "PatientGuardianId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "PatientAddresses",
                columns: new[] { "PatientAddressId", "City", "Country", "CreatedBy", "CreatedDate", "LastModifiedBy", "LastModifiedDate", "State", "StreetAddress", "ZipCode" },
                values: new object[,]
                {
                    { 1, "New York", "USA", 1, new DateTime(2024, 9, 15, 1, 52, 39, 905, DateTimeKind.Local).AddTicks(2372), null, null, "NY", "123 Main St", "10001" },
                    { 2, "Los Angeles", "USA", 1, new DateTime(2024, 9, 15, 1, 52, 39, 905, DateTimeKind.Local).AddTicks(2573), null, null, "CA", "456 Second St", "90001" }
                });

            migrationBuilder.InsertData(
                table: "PatientGuardians",
                columns: new[] { "PatientGuardianId", "PatientGuardianName", "PatientGuardianPhoneNumber", "PatientGuardianRelationship" },
                values: new object[,]
                {
                    { 1, "Jane Doe", "9876543210", "Mother" },
                    { 2, "Sam Smith", "9876543211", "Father" }
                });

            migrationBuilder.InsertData(
                table: "Patients",
                columns: new[] { "PatientId", "Age", "CreatedBy", "CreatedDate", "Dob", "Email", "Gender", "Image", "LastModifiedBy", "LastModifiedDate", "PatientAddressId", "PatientGuardianId", "PatientName", "Phone", "PreferredClinicId", "PreferredDoctorId", "PreferredEndTime", "PreferredStartTime" },
                values: new object[,]
                {
                    { 1, 25, 1, new DateTime(2024, 9, 15, 1, 52, 39, 905, DateTimeKind.Local).AddTicks(3549), new DateTime(1998, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "john.doe@example.com", "Male", null, 1, new DateTime(2024, 9, 15, 1, 52, 39, 905, DateTimeKind.Local).AddTicks(3550), 1, 1, "John Doe", "1234567890", 0, 1, new DateTime(2024, 9, 15, 17, 0, 0, 0, DateTimeKind.Local), new DateTime(2024, 9, 15, 9, 0, 0, 0, DateTimeKind.Local) },
                    { 2, 30, 2, new DateTime(2024, 9, 15, 1, 52, 39, 905, DateTimeKind.Local).AddTicks(3559), new DateTime(1993, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "emily.clark@example.com", "Female", null, 2, new DateTime(2024, 9, 15, 1, 52, 39, 905, DateTimeKind.Local).AddTicks(3561), 2, 2, "Emily Clark", "1234567891", 0, 2, new DateTime(2024, 9, 15, 16, 0, 0, 0, DateTimeKind.Local), new DateTime(2024, 9, 15, 8, 0, 0, 0, DateTimeKind.Local) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Patients_PatientAddressId",
                table: "Patients",
                column: "PatientAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_PatientGuardianId",
                table: "Patients",
                column: "PatientGuardianId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Patients");

            migrationBuilder.DropTable(
                name: "PatientAddresses");

            migrationBuilder.DropTable(
                name: "PatientGuardians");
        }
    }
}
