using ControleFinanceiro.Application.Abstractions;
using ControleFinanceiro.Application.Exceptions;
using ControleFinanceiro.Domain.Profiles;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Application.Services;

public sealed class PerfisServico(IControleFinanceiroDbContext db, IUsuarioAtualServico usuarioAtual)
{
    public async Task<Guid> ObterPerfilPadraoIdAsync(CancellationToken cancellationToken)
    {
        var perfil = await db.Perfis
            .Where(item => item.UsuarioId == usuarioAtual.UsuarioId)
            .OrderByDescending(item => item.Ativo)
            .ThenBy(item => item.CriadoEm)
            .FirstOrDefaultAsync(cancellationToken);

        if (perfil is null)
        {
            perfil = new Perfil("Principal", usuarioAtual.UsuarioId);
            db.Perfis.Add(perfil);
            await db.SaveChangesAsync(cancellationToken);
        }

        return perfil.Id;
    }

    public async Task ValidarPerfilDoUsuarioAsync(Guid perfilId, CancellationToken cancellationToken)
    {
        var pertenceAoUsuario = await db.Perfis.AsNoTracking().AnyAsync(
            item => item.Id == perfilId && item.UsuarioId == usuarioAtual.UsuarioId,
            cancellationToken);

        if (!pertenceAoUsuario)
        {
            throw new NaoEncontradoException();
        }
    }
}
