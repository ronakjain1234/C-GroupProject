using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatabaseHandler.Migrations
{
    /// <inheritdoc />
    public partial class fixednaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CompanyUser",
                table: "CompanyUser");

            migrationBuilder.RenameTable(
                name: "CompanyUser",
                newName: "CompanyUsers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompanyUsers",
                table: "CompanyUsers",
                columns: new[] { "CompanyID", "UserID" });

            migrationBuilder.CreateTable(
                name: "CompanyEndPoints",
                columns: table => new
                {
                    CompanyID = table.Column<int>(type: "INTEGER", nullable: false),
                    EndPointID = table.Column<int>(type: "INTEGER", nullable: false),
                    LastChange = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyEndPoints", x => new { x.CompanyID, x.EndPointID });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyEndPoints");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompanyUsers",
                table: "CompanyUsers");

            migrationBuilder.RenameTable(
                name: "CompanyUsers",
                newName: "CompanyUser");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompanyUser",
                table: "CompanyUser",
                columns: new[] { "CompanyID", "UserID" });
        }
    }
}
