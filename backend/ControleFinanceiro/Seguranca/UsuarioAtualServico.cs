using System.Security.Claims;
using ControleFinanceiro.Application.Abstracoes;
using ControleFinanceiro.Application.Excecoes;
using Microsoft.IdentityModel.JsonWebTokens;

namespace ControleFinanceiro.Seguranca;

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
