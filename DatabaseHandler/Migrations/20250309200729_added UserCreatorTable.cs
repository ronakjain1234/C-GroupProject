using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatabaseHandler.Migrations
{
    /// <inheritdoc />
    public partial class addedUserCreatorTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserCreator",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatorUserID = table.Column<int>(type: "INTEGER", nullable: false),
                    LastChange = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCreator", x => new { x.UserID, x.CreatorUserID });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserCreator");
        }
    }
}
