using ControleFinanceiro.Application.Abstractions;
using ControleFinanceiro.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace ControleFinanceiro.Infrastructure.Security;

public sealed class SenhaServico : ISenhaServico
{
    private readonly PasswordHasher<Usuario> _hasher = new();

    public string GerarHash(Usuario usuario, string senha)
        => _hasher.HashPassword(usuario, senha);

    public bool Verificar(Usuario usuario, string hash, string senha)
    {
        var resultado = _hasher.VerifyHashedPassword(usuario, hash, senha);
        return resultado is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
