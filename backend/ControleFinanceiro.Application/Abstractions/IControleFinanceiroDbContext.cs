using ControleFinanceiro.Domain.Authentication;
using ControleFinanceiro.Domain.CreditCards;
using ControleFinanceiro.Domain.Expenses;
using ControleFinanceiro.Domain.Profiles;
using ControleFinanceiro.Domain.Income;
using ControleFinanceiro.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ControleFinanceiro.Application.Abstractions;

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
