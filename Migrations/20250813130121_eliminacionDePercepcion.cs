using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sistemaFacturacion.Migrations
{
    /// <inheritdoc />
    public partial class eliminacionDePercepcion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PorcentajePercepcion",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "TienePercepcion",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "MontoPercepcion",
                table: "DetalleFacturas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PorcentajePercepcion",
                table: "Productos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "TienePercepcion",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoPercepcion",
                table: "DetalleFacturas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
