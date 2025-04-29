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
            migrationBuilder.DropPrimaryKey(
                name: "PK_EndPointParameters",
                table: "EndPointParameters");

            migrationBuilder.RenameTable(
                name: "EndPointParameters",
                newName: "EndPointParameter");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "EndPoints",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EndPointParameter",
                table: "EndPointParameter",
                columns: new[] { "ParameterID", "EndPointID" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_EndPointParameter",
                table: "EndPointParameter");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "EndPoints");

            migrationBuilder.RenameTable(
                name: "EndPointParameter",
                newName: "EndPointParameters");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EndPointParameters",
                table: "EndPointParameters",
                columns: new[] { "ParameterID", "EndPointID" });
        }
    }
}
