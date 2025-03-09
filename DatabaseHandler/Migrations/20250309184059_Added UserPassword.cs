using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatabaseHandler.Migrations
{
    /// <inheritdoc />
    public partial class AddedUserPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserEmail",
                columns: table => new
                {
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    UserID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserEmail", x => x.Email);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserEmail");
        }
    }
}
