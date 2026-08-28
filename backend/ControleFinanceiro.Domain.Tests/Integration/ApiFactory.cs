using ControleFinanceiro.Application.Abstractions;
using ControleFinanceiro.Infrastructure.Persistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace ControleFinanceiro.Domain.Tests.Integration;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"ControleFinanceiroTests-{Guid.NewGuid()}";

    public ApiFactory()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Host=localhost;Database=controle_financeiro_tests;Username=test;Password=test");
        Environment.SetEnvironmentVariable("Jwt__Chave", "testes-integracao-chave-segura-controle-financeiro-2026");
        Environment.SetEnvironmentVariable("Jwt__Emissor", "ControleFinanceiro");
        Environment.SetEnvironmentVariable("Jwt__Audiencia", "ControleFinanceiro");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
        });
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=controle_financeiro_tests;Username=test;Password=test",
                ["Jwt:Chave"] = "testes-integracao-chave-segura-controle-financeiro-2026",
                ["Jwt:Emissor"] = "ControleFinanceiro",
                ["Jwt:Audiencia"] = "ControleFinanceiro"
            });
        });
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<ControleFinanceiroDbContext>>();
            services.RemoveAll<IControleFinanceiroDbContext>();

            services.AddDataProtection()
                .UseEphemeralDataProtectionProvider();

            services.AddDbContext<ControleFinanceiroDbContext>(options =>
                options
                    .UseInMemoryDatabase(_databaseName)
                    .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)));

            services.AddScoped<IControleFinanceiroDbContext>(provider =>
                provider.GetRequiredService<ControleFinanceiroDbContext>());
        });
    }
}
