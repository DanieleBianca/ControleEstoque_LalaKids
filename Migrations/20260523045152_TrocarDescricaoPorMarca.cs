using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoftwareLalaKids.Migrations
{
    /// <inheritdoc />
    public partial class TrocarDescricaoPorMarca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Descricao",
                table: "Produto",
                newName: "Marca");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Marca",
                table: "Produto",
                newName: "Descricao");
        }
    }
}
