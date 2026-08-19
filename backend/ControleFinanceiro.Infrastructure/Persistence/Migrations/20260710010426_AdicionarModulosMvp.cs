using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarModulosMvp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CartoesCredito",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Banco = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Bandeira = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Limite = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DiaFechamento = table.Column<int>(type: "integer", nullable: false),
                    DiaVencimento = table.Column<int>(type: "integer", nullable: false),
                    Cor = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    PerfilId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartoesCredito", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartoesCredito_Perfis_PerfilId",
                        column: x => x.PerfilId,
                        principalTable: "Perfis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CategoriasDespesa",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PerfilId = table.Column<Guid>(type: "uuid", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasDespesa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoriasDespesa_Perfis_PerfilId",
                        column: x => x.PerfilId,
                        principalTable: "Perfis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Receitas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DataRecebimento = table.Column<DateOnly>(type: "date", nullable: false),
                    Competencia = table.Column<DateOnly>(type: "date", nullable: false),
                    Observacoes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PerfilId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receitas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Receitas_Perfis_PerfilId",
                        column: x => x.PerfilId,
                        principalTable: "Perfis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioRefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExpiraEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevogadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubstituidoPorTokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioRefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuarioRefreshTokens_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComprasCartao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    ValorTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DataCompra = table.Column<DateOnly>(type: "date", nullable: false),
                    QuantidadeParcelas = table.Column<int>(type: "integer", nullable: false),
                    CartaoCreditoId = table.Column<Guid>(type: "uuid", nullable: false),
                    PerfilId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComprasCartao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComprasCartao_CartoesCredito_CartaoCreditoId",
                        column: x => x.CartaoCreditoId,
                        principalTable: "CartoesCredito",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComprasCartao_Perfis_PerfilId",
                        column: x => x.PerfilId,
                        principalTable: "Perfis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FaturasCartao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CartaoCreditoId = table.Column<Guid>(type: "uuid", nullable: false),
                    PerfilId = table.Column<Guid>(type: "uuid", nullable: false),
                    MesReferencia = table.Column<DateOnly>(type: "date", nullable: false),
                    DataFechamento = table.Column<DateOnly>(type: "date", nullable: false),
                    DataVencimento = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaturasCartao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaturasCartao_CartoesCredito_CartaoCreditoId",
                        column: x => x.CartaoCreditoId,
                        principalTable: "CartoesCredito",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FaturasCartao_Perfis_PerfilId",
                        column: x => x.PerfilId,
                        principalTable: "Perfis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Despesas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DataVencimento = table.Column<DateOnly>(type: "date", nullable: false),
                    DataPagamento = table.Column<DateOnly>(type: "date", nullable: true),
                    Competencia = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Observacoes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CategoriaDespesaId = table.Column<Guid>(type: "uuid", nullable: false),
                    PerfilId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Despesas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Despesas_CategoriasDespesa_CategoriaDespesaId",
                        column: x => x.CategoriaDespesaId,
                        principalTable: "CategoriasDespesa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Despesas_Perfis_PerfilId",
                        column: x => x.PerfilId,
                        principalTable: "Perfis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ParcelasCartao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompraCartaoId = table.Column<Guid>(type: "uuid", nullable: false),
                    FaturaCartaoId = table.Column<Guid>(type: "uuid", nullable: false),
                    NumeroParcela = table.Column<int>(type: "integer", nullable: false),
                    QuantidadeParcelas = table.Column<int>(type: "integer", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelasCartao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParcelasCartao_ComprasCartao_CompraCartaoId",
                        column: x => x.CompraCartaoId,
                        principalTable: "ComprasCartao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParcelasCartao_FaturasCartao_FaturaCartaoId",
                        column: x => x.FaturaCartaoId,
                        principalTable: "FaturasCartao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartoesCredito_PerfilId",
                table: "CartoesCredito",
                column: "PerfilId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasDespesa_PerfilId_Nome",
                table: "CategoriasDespesa",
                columns: new[] { "PerfilId", "Nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComprasCartao_CartaoCreditoId",
                table: "ComprasCartao",
                column: "CartaoCreditoId");

            migrationBuilder.CreateIndex(
                name: "IX_ComprasCartao_PerfilId_CartaoCreditoId",
                table: "ComprasCartao",
                columns: new[] { "PerfilId", "CartaoCreditoId" });

            migrationBuilder.CreateIndex(
                name: "IX_Despesas_CategoriaDespesaId",
                table: "Despesas",
                column: "CategoriaDespesaId");

            migrationBuilder.CreateIndex(
                name: "IX_Despesas_PerfilId_Competencia",
                table: "Despesas",
                columns: new[] { "PerfilId", "Competencia" });

            migrationBuilder.CreateIndex(
                name: "IX_FaturasCartao_CartaoCreditoId_MesReferencia",
                table: "FaturasCartao",
                columns: new[] { "CartaoCreditoId", "MesReferencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FaturasCartao_PerfilId_MesReferencia",
                table: "FaturasCartao",
                columns: new[] { "PerfilId", "MesReferencia" });

            migrationBuilder.CreateIndex(
                name: "IX_ParcelasCartao_CompraCartaoId",
                table: "ParcelasCartao",
                column: "CompraCartaoId");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelasCartao_FaturaCartaoId",
                table: "ParcelasCartao",
                column: "FaturaCartaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Receitas_PerfilId_Competencia",
                table: "Receitas",
                columns: new[] { "PerfilId", "Competencia" });

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioRefreshTokens_TokenHash",
                table: "UsuarioRefreshTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioRefreshTokens_UsuarioId",
                table: "UsuarioRefreshTokens",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Despesas");

            migrationBuilder.DropTable(
                name: "ParcelasCartao");

            migrationBuilder.DropTable(
                name: "Receitas");

            migrationBuilder.DropTable(
                name: "UsuarioRefreshTokens");

            migrationBuilder.DropTable(
                name: "CategoriasDespesa");

            migrationBuilder.DropTable(
                name: "ComprasCartao");

            migrationBuilder.DropTable(
                name: "FaturasCartao");

            migrationBuilder.DropTable(
                name: "CartoesCredito");
        }
    }
}
