using ControleFinanceiro.Infrastructure.InjecaoDependencia;
using ControleFinanceiro.Infrastructure.Seguranca;
using ControleFinanceiro.Middlewares;
using ControleFinanceiro.Seguranca;
using ControleFinanceiro.Application.Abstracoes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

app.MapControllers();

app.Run();

public partial class Program;
