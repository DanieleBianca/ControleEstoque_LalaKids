using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoftwareLalaKids.Migrations
{
    /// <inheritdoc />
    public partial class AddMovimentacaoItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdProduto",
                table: "Movimentacao");

            migrationBuilder.DropColumn(
                name: "Quantidade",
                table: "Movimentacao");

            migrationBuilder.DropColumn(
                name: "Tamanho",
                table: "Movimentacao");

            migrationBuilder.CreateTable(
                name: "MovimentacaoItem",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IdMovimentacao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdProduto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tamanho = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantidade = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimentacaoItem", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovimentacaoItem");

            migrationBuilder.AddColumn<string>(
                name: "IdProduto",
                table: "Movimentacao",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Quantidade",
                table: "Movimentacao",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Tamanho",
                table: "Movimentacao",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
