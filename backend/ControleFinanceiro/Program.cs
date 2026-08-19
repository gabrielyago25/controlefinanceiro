using ControleFinanceiro.Infrastructure.DependencyInjection;
using ControleFinanceiro.Infrastructure.Persistence;
using ControleFinanceiro.Infrastructure.Security;
using ControleFinanceiro.Middleware;
using ControleFinanceiro.Security;
using ControleFinanceiro.Application.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AdicionarServicosInfraestrutura(builder.Configuration);
}
else
{
    builder.Services.AdicionarInfraestrutura(builder.Configuration);
}

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUsuarioAtualServico, UsuarioAtualServico>();

var jwtOpcoes = builder.Configuration.GetSection(JwtOpcoes.Secao).Get<JwtOpcoes>() ?? new JwtOpcoes();
var chaveJwt = jwtOpcoes.Chave;
if (string.IsNullOrWhiteSpace(chaveJwt) && builder.Environment.IsDevelopment())
{
    chaveJwt = "desenvolvimento-local-chave-efemera-controle-financeiro-2026";
    builder.Services.PostConfigure<JwtOpcoes>(opcoes => opcoes.Chave = chaveJwt);
}

if (string.IsNullOrWhiteSpace(chaveJwt))
{
    throw new InvalidOperationException("Configure Jwt:Chave por User Secrets, variável de ambiente ou provedor seguro.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOpcoes.Emissor,
            ValidateAudience = true,
            ValidAudience = jwtOpcoes.Audiencia,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveJwt)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = JwtRegisteredClaimNames.Sub
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:OrigensPermitidas").Get<string[]>() ?? ["http://localhost:5173"])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (builder.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ControleFinanceiroDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseMiddleware<TratamentoErrosMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.UseCors("Frontend");

app.UseAuthentication();

app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

app.MapControllers();

app.Run();

public partial class Program;
