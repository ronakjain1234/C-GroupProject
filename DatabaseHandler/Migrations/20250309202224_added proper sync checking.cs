using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatabaseHandler.Migrations
{
    /// <inheritdoc />
    public partial class addedpropersyncchecking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastChange",
                table: "UserEmail",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastChange",
                table: "UserActive",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastChange",
                table: "Modules",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "UserPassword",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "INTEGER", nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: false),
                    LastChange = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPassword", x => new { x.UserID, x.Password });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPassword");

            migrationBuilder.DropColumn(
                name: "LastChange",
                table: "UserEmail");

            migrationBuilder.DropColumn(
                name: "LastChange",
                table: "UserActive");

            migrationBuilder.DropColumn(
                name: "LastChange",
                table: "Modules");
        }
    }
}
