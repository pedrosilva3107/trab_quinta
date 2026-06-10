using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmaciaSistema.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Farmaceuticos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    SenhaHash = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Farmaceuticos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Produtos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    CodigoBarras = table.Column<string>(type: "TEXT", nullable: false),
                    PrincipioAtivo = table.Column<string>(type: "TEXT", nullable: true),
                    TipoProduto = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produtos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vendas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DataVenda = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LotesEstoque",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProdutoId = table.Column<int>(type: "INTEGER", nullable: false),
                    NumeroLote = table.Column<string>(type: "TEXT", nullable: false),
                    DataValidade = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Quantidade = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LotesEstoque", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LotesEstoque_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RetencoesSngpc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProdutoId = table.Column<int>(type: "INTEGER", nullable: false),
                    NumeroLote = table.Column<string>(type: "TEXT", nullable: false),
                    DataValidade = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Quantidade = table.Column<int>(type: "INTEGER", nullable: false),
                    TipoMovimento = table.Column<string>(type: "TEXT", nullable: false),
                    DataMovimento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FarmaceuticoId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReceitaPacienteCriptografado = table.Column<string>(type: "TEXT", nullable: false),
                    TransmitidoSngpc = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetencoesSngpc", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RetencoesSngpc_Farmaceuticos_FarmaceuticoId",
                        column: x => x.FarmaceuticoId,
                        principalTable: "Farmaceuticos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RetencoesSngpc_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItensVenda",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VendaId = table.Column<int>(type: "INTEGER", nullable: false),
                    LoteEstoqueId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantidade = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensVenda", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItensVenda_LotesEstoque_LoteEstoqueId",
                        column: x => x.LoteEstoqueId,
                        principalTable: "LotesEstoque",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItensVenda_Vendas_VendaId",
                        column: x => x.VendaId,
                        principalTable: "Vendas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Farmaceuticos",
                columns: new[] { "Id", "Nome", "SenhaHash" },
                values: new object[] { 1, "ana", "03AC674216F3E15C761EE1A5E255F067953623C8B388B4459E13F978D7C846F4" });

            migrationBuilder.CreateIndex(
                name: "IX_ItensVenda_LoteEstoqueId",
                table: "ItensVenda",
                column: "LoteEstoqueId");

            migrationBuilder.CreateIndex(
                name: "IX_ItensVenda_VendaId",
                table: "ItensVenda",
                column: "VendaId");

            migrationBuilder.CreateIndex(
                name: "IX_LotesEstoque_ProdutoId",
                table: "LotesEstoque",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Produtos_CodigoBarras",
                table: "Produtos",
                column: "CodigoBarras",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RetencoesSngpc_FarmaceuticoId",
                table: "RetencoesSngpc",
                column: "FarmaceuticoId");

            migrationBuilder.CreateIndex(
                name: "IX_RetencoesSngpc_ProdutoId",
                table: "RetencoesSngpc",
                column: "ProdutoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItensVenda");

            migrationBuilder.DropTable(
                name: "RetencoesSngpc");

            migrationBuilder.DropTable(
                name: "LotesEstoque");

            migrationBuilder.DropTable(
                name: "Vendas");

            migrationBuilder.DropTable(
                name: "Farmaceuticos");

            migrationBuilder.DropTable(
                name: "Produtos");
        }
    }
}
