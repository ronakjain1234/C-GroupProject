using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatabaseHandler.Migrations
{
    /// <inheritdoc />
    public partial class fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Parameters");

            migrationBuilder.RenameColumn(
                name: "Path",
                table: "EndPoints",
                newName: "Specification");

            migrationBuilder.AddColumn<string>(
                name: "EndPointName",
                table: "EndPoints",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndPointName",
                table: "EndPoints");

            migrationBuilder.RenameColumn(
                name: "Specification",
                table: "EndPoints",
                newName: "Path");

            migrationBuilder.CreateTable(
                name: "Parameters",
                columns: table => new
                {
                    ParameterID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LastChange = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ParameterName = table.Column<string>(type: "TEXT", nullable: false),
                    ParameterType = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parameters", x => x.ParameterID);
                });
        }
    }
}
