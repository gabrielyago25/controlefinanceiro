namespace ControleFinanceiro.Infrastructure.Security;

public sealed class JwtOpcoes
{
    public const string Secao = "Jwt";

    public string Emissor { get; set; } = "ControleFinanceiro";
    public string Audiencia { get; set; } = "ControleFinanceiro";
    public string Chave { get; set; } = string.Empty;
    public int AccessTokenMinutos { get; set; } = 15;
    public int RefreshTokenDias { get; set; } = 7;
}
