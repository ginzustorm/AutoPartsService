using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoPartsServiceWebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddedAvatarsMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Avatar",
                table: "UserCommons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Avatar",
                table: "UserBusinesses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Avatar",
                table: "UserCommons");

            migrationBuilder.DropColumn(
                name: "Avatar",
                table: "UserBusinesses");
        }
    }
}
