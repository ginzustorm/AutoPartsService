using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoPartsServiceWebApi.Migrations
{
    /// <inheritdoc />
    public partial class Migration250523 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Services_UserBusinessId",
                table: "Services");

            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "UserBusinesses",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.AddColumn<int>(
                name: "ServiceType",
                table: "Services",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    UserBusinessId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_UserBusinesses_UserBusinessId",
                        column: x => x.UserBusinessId,
                        principalTable: "UserBusinesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Services_UserBusinessId",
                table: "Services",
                column: "UserBusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserBusinessId",
                table: "Reviews",
                column: "UserBusinessId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Services_UserBusinessId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "UserBusinesses");

            migrationBuilder.DropColumn(
                name: "PartManufacturer",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "PartType",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "ServiceType",
                table: "Services");

            migrationBuilder.CreateIndex(
                name: "IX_Services_UserBusinessId",
                table: "Services",
                column: "UserBusinessId",
                unique: true);
        }
    }
}
