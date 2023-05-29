using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoPartsServiceWebApi.Migrations
{
    /// <inheritdoc />
    public partial class Migration2505232 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PartManufacturer",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "PartType",
                table: "Services");

            migrationBuilder.AddColumn<int>(
                name: "AcceptedByUserId",
                table: "Requests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "Requests",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcceptedByUserId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "Active",
                table: "Requests");

            migrationBuilder.AddColumn<string>(
                name: "PartManufacturer",
                table: "Services",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PartType",
                table: "Services",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
