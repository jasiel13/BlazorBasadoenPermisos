using Microsoft.EntityFrameworkCore.Migrations;

namespace PriceGas.Server.Migrations
{
    public partial class MultiplesImg : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Imagen",
                table: "Carrusel",
                newName: "Descripcion");

            migrationBuilder.AddColumn<int>(
                name: "LugardeVisualizacion",
                table: "Carrusel",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Mostrar",
                table: "Carrusel",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ImagenesCarrusel",
                columns: table => new
                {
                    ImagenesCarruselId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Imagen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CarruselId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImagenesCarrusel", x => x.ImagenesCarruselId);
                    table.ForeignKey(
                        name: "FK_ImagenesCarrusel_Carrusel_CarruselId",
                        column: x => x.CarruselId,
                        principalTable: "Carrusel",
                        principalColumn: "CarruselId",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_ImagenesCarrusel_CarruselId",
                table: "ImagenesCarrusel",
                column: "CarruselId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImagenesCarrusel");

            migrationBuilder.DropColumn(
                name: "LugardeVisualizacion",
                table: "Carrusel");

            migrationBuilder.DropColumn(
                name: "Mostrar",
                table: "Carrusel");

            migrationBuilder.RenameColumn(
                name: "Descripcion",
                table: "Carrusel",
                newName: "Imagen");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "06a0e103-5645-40dd-b94c-044d6573821c",
                column: "ConcurrencyStamp",
                value: "8a51876b-7949-4267-867c-833635620621");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "54202d5b-deb1-417d-a05e-5b2d8fe48e4d",
                column: "ConcurrencyStamp",
                value: "c778241b-b38a-453f-a458-2b788428f744");
        }
    }
}
