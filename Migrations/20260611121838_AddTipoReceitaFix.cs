using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmaciaSistema.Migrations
{
    /// <inheritdoc />
    public partial class AddTipoReceitaFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TipoReceita",
                table: "Produtos",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipoReceita",
                table: "Produtos");
        }
    }
}
