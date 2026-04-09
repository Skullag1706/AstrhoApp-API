using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstrhoApp.API.Migrations
{
    /// <inheritdoc />
    public partial class observacioncompra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Observacion",
                table: "Compra",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Observacion",
                table: "Compra");
        }
    }
}
