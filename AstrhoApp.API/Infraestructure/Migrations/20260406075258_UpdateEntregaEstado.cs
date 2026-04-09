using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstrhoApp.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntregaEstado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "estado",
                table: "Entregainsumo");

            migrationBuilder.AddColumn<int>(
                name: "estado_id",
                table: "Entregainsumo",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Entregainsumo_estado_id",
                table: "Entregainsumo",
                column: "estado_id");

            migrationBuilder.AddForeignKey(
                name: "FK_EntregaIn_Estado",
                table: "Entregainsumo",
                column: "estado_id",
                principalTable: "Estado",
                principalColumn: "estado_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EntregaIn_Estado",
                table: "Entregainsumo");

            migrationBuilder.DropIndex(
                name: "IX_Entregainsumo_estado_id",
                table: "Entregainsumo");

            migrationBuilder.DropColumn(
                name: "estado_id",
                table: "Entregainsumo");

            migrationBuilder.AddColumn<bool>(
                name: "estado",
                table: "Entregainsumo",
                type: "bit",
                nullable: true,
                defaultValue: true);
        }
    }
}
