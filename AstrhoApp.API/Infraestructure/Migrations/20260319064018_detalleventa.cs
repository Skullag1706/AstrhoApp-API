using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstrhoApp.API.Migrations
{
    /// <inheritdoc />
    public partial class detalleventa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DetalleVenta",
                columns: table => new
                {
                    detalleVenta_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    venta_id = table.Column<string>(type: "varchar(25)", unicode: false, maxLength: 25, nullable: false),
                    servicio_id = table.Column<int>(type: "int", nullable: false),
                    precio = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleVenta", x => x.detalleVenta_id);
                    table.ForeignKey(
                        name: "FK_DetalleVenta_Servicio",
                        column: x => x.servicio_id,
                        principalTable: "Servicio",
                        principalColumn: "servicio_id");
                    table.ForeignKey(
                        name: "FK_DetalleVenta_Venta",
                        column: x => x.venta_id,
                        principalTable: "Venta",
                        principalColumn: "venta_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetalleVenta_servicio_id",
                table: "DetalleVenta",
                column: "servicio_id");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleVenta_venta_id",
                table: "DetalleVenta",
                column: "venta_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetalleVenta");
        }
    }
}
