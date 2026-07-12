using ControleFinanceiro.Domain.Autenticacao;
using ControleFinanceiro.Domain.Cartoes;
using ControleFinanceiro.Domain.Despesas;
using ControleFinanceiro.Domain.Perfis;
using ControleFinanceiro.Domain.Receitas;
using ControleFinanceiro.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ControleFinanceiro.Application.Abstracoes;

public interface IControleFinanceiroDbContext
{
    DbSet<Usuario> Usuarios { get; }
    DbSet<Perfil> Perfis { get; }
    DbSet<UsuarioRefreshToken> UsuarioRefreshTokens { get; }
    DbSet<CategoriaDespesa> CategoriasDespesa { get; }
    DbSet<Despesa> Despesas { get; }
    DbSet<Receita> Receitas { get; }
    DbSet<CartaoCredito> CartoesCredito { get; }
    DbSet<CompraCartao> ComprasCartao { get; }
    DbSet<ParcelaCartao> ParcelasCartao { get; }
    DbSet<FaturaCartao> FaturasCartao { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
