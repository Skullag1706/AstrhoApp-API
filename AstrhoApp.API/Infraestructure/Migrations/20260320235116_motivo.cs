using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstrhoApp.API.Migrations
{
    /// <inheritdoc />
    public partial class motivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Motivos",
                columns: table => new
                {
                    MotivoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "time", nullable: false),
                    HoraFin = table.Column<TimeSpan>(type: "time", nullable: false),
                    DocumentoEmpleado = table.Column<string>(type: "varchar(20)", nullable: false),
                    EstadoId = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Motivos", x => x.MotivoId);
                    table.ForeignKey(
                        name: "FK_Motivos_Empleado_DocumentoEmpleado",
                        column: x => x.DocumentoEmpleado,
                        principalTable: "Empleado",
                        principalColumn: "documento_empleado",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Motivos_Estado_EstadoId",
                        column: x => x.EstadoId,
                        principalTable: "Estado",
                        principalColumn: "estado_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Motivos_DocumentoEmpleado",
                table: "Motivos",
                column: "DocumentoEmpleado");

            migrationBuilder.CreateIndex(
                name: "IX_Motivos_EstadoId",
                table: "Motivos",
                column: "EstadoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Motivos");
        }
    }
}
