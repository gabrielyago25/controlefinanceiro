using ControleFinanceiro.Application.Abstractions;
using ControleFinanceiro.Application.Services;
using ControleFinanceiro.Infrastructure.Persistence;
using ControleFinanceiro.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ControleFinanceiro.Infrastructure.DependencyInjection;

public static class ConfiguracaoDependencias
{
    public static IServiceCollection AdicionarInfraestrutura(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(
            "DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "A connection string 'DefaultConnection' não foi configurada.");
        }

        services.AddDbContext<ControleFinanceiroDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IControleFinanceiroDbContext>(provider =>
            provider.GetRequiredService<ControleFinanceiroDbContext>());

        services.AdicionarServicosInfraestrutura(configuration);

        return services;
    }

    public static IServiceCollection AdicionarServicosInfraestrutura(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<ISenhaServico, SenhaServico>();
        services.AddScoped<ITokenServico, TokenServico>();
        services.AddScoped<AutenticacaoServico>();
        services.AddScoped<PerfisServico>();
        services.AddScoped<FinanceiroServico>();
        services.AddScoped<CartoesServico>();
        services.Configure<JwtOpcoes>(configuration.GetSection(JwtOpcoes.Secao));

        return services;
    }
}
