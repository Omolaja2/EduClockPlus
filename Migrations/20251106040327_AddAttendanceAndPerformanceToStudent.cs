using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduClockPlus.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceAndPerformanceToStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Attendance",
                table: "Students",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Performance",
                table: "Students",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Subjects",
                table: "Students",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Attendance",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Performance",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Subjects",
                table: "Students");
        }
    }
}
