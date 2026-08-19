using System.Security.Claims;
using ControleFinanceiro.Application.Abstractions;
using ControleFinanceiro.Application.Exceptions;
using Microsoft.IdentityModel.JsonWebTokens;

namespace ControleFinanceiro.Security;

public sealed class UsuarioAtualServico(IHttpContextAccessor httpContextAccessor) : IUsuarioAtualServico
{
    public Guid UsuarioId
    {
        get
        {
            var usuario = httpContextAccessor.HttpContext?.User;
            var id = usuario?.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? usuario?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(id, out var usuarioId))
            {
                throw new NaoAutenticadoException();
            }

            return usuarioId;
        }
    }
}
