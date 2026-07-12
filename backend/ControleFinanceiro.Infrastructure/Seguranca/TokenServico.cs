using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ControleFinanceiro.Application.Abstracoes;
using ControleFinanceiro.Domain.Usuarios;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ControleFinanceiro.Infrastructure.Seguranca;

public sealed class TokenServico(IOptions<JwtOpcoes> opcoes) : ITokenServico
{
    public TokenEmitido EmitirTokens(Usuario usuario)
    {
        var jwtOpcoes = opcoes.Value;
        if (string.IsNullOrWhiteSpace(jwtOpcoes.Chave) || jwtOpcoes.Chave.Length < 32)
        {
            throw new InvalidOperationException("A chave JWT deve possuir ao menos 32 caracteres.");
        }

        var agora = DateTime.UtcNow;
        var expiraEm = agora.AddMinutes(jwtOpcoes.AccessTokenMinutos);
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOpcoes.Chave));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = jwtOpcoes.Emissor,
            Audience = jwtOpcoes.Audiencia,
            Expires = expiraEm,
            NotBefore = agora,
            IssuedAt = agora,
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim(JwtRegisteredClaimNames.Name, usuario.Nome)
            ]),
            SigningCredentials = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256)
        };

        var accessToken = new JsonWebTokenHandler().CreateToken(descriptor);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        return new TokenEmitido(accessToken, expiraEm, refreshToken, agora.AddDays(jwtOpcoes.RefreshTokenDias));
    }

    public string GerarHashRefreshToken(string refreshToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(bytes);
    }
}
