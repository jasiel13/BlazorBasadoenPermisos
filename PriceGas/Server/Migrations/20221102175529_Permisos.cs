using Microsoft.EntityFrameworkCore.Migrations;

namespace PriceGas.Server.Migrations
{
    public partial class Permisos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "06a0e103-5645-40dd-b94c-044d6573821c",
                column: "ConcurrencyStamp",
                value: "db73b093-a8d8-47d0-8c0e-7d20346748fb");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "54202d5b-deb1-417d-a05e-5b2d8fe48e4d",
                column: "ConcurrencyStamp",
                value: "de27da23-1e67-4879-ba31-3adc61a17ed3");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "06a0e103-5645-40dd-b94c-044d6573821c",
                column: "ConcurrencyStamp",
                value: "fc65e432-2417-4fcf-bcb7-d687e4b383da");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "54202d5b-deb1-417d-a05e-5b2d8fe48e4d",
                column: "ConcurrencyStamp",
                value: "dff33352-9ba1-4a33-8203-1d79fc11060c");
        }
    }
}
