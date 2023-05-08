using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoPartsServiceWebApi.Migrations
{
    /// <inheritdoc />
    public partial class RemovedRequiredMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "RegistrationDate",
                value: new DateTime(2023, 5, 8, 19, 13, 42, 29, DateTimeKind.Local).AddTicks(9019));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "RegistrationDate",
                value: new DateTime(2023, 5, 8, 19, 13, 42, 29, DateTimeKind.Local).AddTicks(9032));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "RegistrationDate",
                value: new DateTime(2023, 5, 8, 14, 28, 30, 579, DateTimeKind.Local).AddTicks(5158));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "RegistrationDate",
                value: new DateTime(2023, 5, 8, 14, 28, 30, 579, DateTimeKind.Local).AddTicks(5168));
        }
    }
}
