using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIBarbearia.Migrations
{
    /// <inheritdoc />
    public partial class AddDiasBloqueados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiasBloqueados",
                columns: table => new
                {
                    DiaBloqueadoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    Data = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Motivo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiasBloqueados", x => x.DiaBloqueadoId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiasBloqueados");
        }
    }
}
