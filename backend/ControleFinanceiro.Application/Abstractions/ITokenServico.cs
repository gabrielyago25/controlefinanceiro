using ControleFinanceiro.Domain.Users;

namespace ControleFinanceiro.Application.Abstractions;

public sealed record TokenEmitido(string AccessToken, DateTime ExpiraEm, string RefreshToken, DateTime RefreshTokenExpiraEm);

public interface ITokenServico
{
    TokenEmitido EmitirTokens(Usuario usuario);
    string GerarHashRefreshToken(string refreshToken);
}
