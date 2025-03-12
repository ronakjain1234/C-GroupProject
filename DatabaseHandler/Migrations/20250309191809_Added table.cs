using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatabaseHandler.Migrations
{
    /// <inheritdoc />
    public partial class Addedtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModuleEndPoints",
                columns: table => new
                {
                    ModuleID = table.Column<int>(type: "INTEGER", nullable: false),
                    EndPointID = table.Column<int>(type: "INTEGER", nullable: false),
                    LastChange = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleEndPoints", x => new { x.ModuleID, x.EndPointID });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModuleEndPoints");
        }
    }
}
