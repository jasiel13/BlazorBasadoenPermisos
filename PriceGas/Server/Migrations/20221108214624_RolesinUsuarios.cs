using Microsoft.EntityFrameworkCore.Migrations;

namespace PriceGas.Server.Migrations
{
    public partial class RolesinUsuarios : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "06a0e103-5645-40dd-b94c-044d6573821c");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "54202d5b-deb1-417d-a05e-5b2d8fe48e4d");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "54202d5b-deb1-417d-a05e-5b2d8fe48e4d", "de27da23-1e67-4879-ba31-3adc61a17ed3", "Administrador", "Administrador" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "06a0e103-5645-40dd-b94c-044d6573821c", "db73b093-a8d8-47d0-8c0e-7d20346748fb", "Usuario", "Usuario" });
        }
    }
}
