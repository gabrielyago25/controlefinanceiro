using ControleFinanceiro.Application.Abstractions;
using ControleFinanceiro.Domain.Authentication;
using ControleFinanceiro.Domain.CreditCards;
using ControleFinanceiro.Domain.Expenses;
using ControleFinanceiro.Domain.Profiles;
using ControleFinanceiro.Domain.Income;
using ControleFinanceiro.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ControleFinanceiro.Infrastructure.Persistence;

public class ControleFinanceiroDbContext : DbContext, IControleFinanceiroDbContext
{
    public ControleFinanceiroDbContext(
        DbContextOptions<ControleFinanceiroDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Perfil> Perfis => Set<Perfil>();

    public DbSet<UsuarioRefreshToken> UsuarioRefreshTokens => Set<UsuarioRefreshToken>();

    public DbSet<CategoriaDespesa> CategoriasDespesa => Set<CategoriaDespesa>();

    public DbSet<Despesa> Despesas => Set<Despesa>();

    public DbSet<Receita> Receitas => Set<Receita>();

    public DbSet<CartaoCredito> CartoesCredito => Set<CartaoCredito>();

    public DbSet<CompraCartao> ComprasCartao => Set<CompraCartao>();

    public DbSet<ParcelaCartao> ParcelasCartao => Set<ParcelaCartao>();

    public DbSet<FaturaCartao> FaturasCartao => Set<FaturaCartao>();

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        => Database.BeginTransactionAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ControleFinanceiroDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
