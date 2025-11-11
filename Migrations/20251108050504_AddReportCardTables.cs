using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduClockPlus.Migrations
{
    /// <inheritdoc />
    public partial class AddReportCardTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportSubjects",
                columns: table => new
                {
                    SubjectID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ReportID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SubjectName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Score = table.Column<int>(type: "int", nullable: false),
                    Grade = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportSubjects", x => x.SubjectID);
                    table.ForeignKey(
                        name: "FK_ReportSubjects_ReportCards_ReportID",
                        column: x => x.ReportID,
                        principalTable: "ReportCards",
                        principalColumn: "ReportID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ReportSubjects_ReportID",
                table: "ReportSubjects",
                column: "ReportID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportSubjects");
        }
    }
}
