using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstrhoApp.API.Migrations
{
    /// <inheritdoc />
    public partial class RedesignHorarioSemanal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__HorarioEm__horar__04E4BC85",
                table: "HorarioEmpleado");

            migrationBuilder.DropPrimaryKey(
                name: "PK__HorarioE__2DCE8B136FB204D3",
                table: "HorarioEmpleado");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Horario__5A3872284E751019",
                table: "Horario");

            migrationBuilder.DropColumn(
                name: "dia_semana",
                table: "Horario");

            migrationBuilder.DropColumn(
                name: "hora_fin",
                table: "Horario");

            migrationBuilder.DropColumn(
                name: "hora_inicio",
                table: "Horario");

            migrationBuilder.RenameColumn(
                name: "horario_id",
                table: "HorarioEmpleado",
                newName: "horarioDia_id");

            migrationBuilder.AddColumn<string>(
                name: "nombre",
                table: "Horario",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HorarioEmpleado",
                table: "HorarioEmpleado",
                column: "horarioEmpleado_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Horario",
                table: "Horario",
                column: "horario_id");

            migrationBuilder.CreateTable(
                name: "HorarioDia",
                columns: table => new
                {
                    horarioDia_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    horario_id = table.Column<int>(type: "int", nullable: false),
                    dia_semana = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false),
                    hora_inicio = table.Column<TimeOnly>(type: "time", nullable: false),
                    hora_fin = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorarioDia", x => x.horarioDia_id);
                    table.ForeignKey(
                        name: "FK_HorarioDia_Horario",
                        column: x => x.horario_id,
                        principalTable: "Horario",
                        principalColumn: "horario_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_HorarioDia_Empleado",
                table: "HorarioEmpleado",
                columns: new[] { "horarioDia_id", "documento_empleado" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HorarioDia_horario_id",
                table: "HorarioDia",
                column: "horario_id");

            migrationBuilder.AddForeignKey(
                name: "FK_HorarioEmp_HorarioDia",
                table: "HorarioEmpleado",
                column: "horarioDia_id",
                principalTable: "HorarioDia",
                principalColumn: "horarioDia_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HorarioEmp_HorarioDia",
                table: "HorarioEmpleado");

            migrationBuilder.DropTable(
                name: "HorarioDia");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HorarioEmpleado",
                table: "HorarioEmpleado");

            migrationBuilder.DropIndex(
                name: "UQ_HorarioDia_Empleado",
                table: "HorarioEmpleado");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Horario",
                table: "Horario");

            migrationBuilder.DropColumn(
                name: "nombre",
                table: "Horario");

            migrationBuilder.RenameColumn(
                name: "horarioDia_id",
                table: "HorarioEmpleado",
                newName: "horario_id");

            migrationBuilder.AddColumn<string>(
                name: "dia_semana",
                table: "Horario",
                type: "varchar(15)",
                unicode: false,
                maxLength: 15,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "hora_fin",
                table: "Horario",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "hora_inicio",
                table: "Horario",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddPrimaryKey(
                name: "PK__HorarioE__2DCE8B136FB204D3",
                table: "HorarioEmpleado",
                column: "horarioEmpleado_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Horario__5A3872284E751019",
                table: "Horario",
                column: "horario_id");

            migrationBuilder.AddForeignKey(
                name: "FK__HorarioEm__horar__04E4BC85",
                table: "HorarioEmpleado",
                column: "horario_id",
                principalTable: "Horario",
                principalColumn: "horario_id");
        }
    }
}
