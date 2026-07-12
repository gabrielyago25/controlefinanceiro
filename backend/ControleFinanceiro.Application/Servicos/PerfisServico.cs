using ControleFinanceiro.Application.Abstracoes;
using ControleFinanceiro.Application.Dtos;
using ControleFinanceiro.Application.Excecoes;
using ControleFinanceiro.Domain.Perfis;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Application.Servicos;

public sealed class PerfisServico(IControleFinanceiroDbContext db, IUsuarioAtualServico usuarioAtual)
{
    public async Task<IReadOnlyList<PerfilResponse>> ListarAsync(CancellationToken cancellationToken)
        => await db.Perfis.AsNoTracking()
            .Where(perfil => perfil.UsuarioId == usuarioAtual.UsuarioId)
            .OrderBy(perfil => perfil.Nome)
            .Select(perfil => new PerfilResponse(perfil.Id, perfil.Nome, perfil.CodigoMoeda, perfil.Ativo, perfil.CriadoEm))
            .ToListAsync(cancellationToken);

    public async Task<PerfilResponse> CriarAsync(CriarPerfilRequest request, CancellationToken cancellationToken)
    {
        var perfil = new Perfil(request.Nome, usuarioAtual.UsuarioId, string.IsNullOrWhiteSpace(request.CodigoMoeda) ? "BRL" : request.CodigoMoeda);
        db.Perfis.Add(perfil);
        await db.SaveChangesAsync(cancellationToken);
        return new PerfilResponse(perfil.Id, perfil.Nome, perfil.CodigoMoeda, perfil.Ativo, perfil.CriadoEm);
    }

    public async Task<PerfilResponse> AlterarAsync(Guid id, AlterarPerfilRequest request, CancellationToken cancellationToken)
    {
        var perfil = await ObterPerfilDoUsuarioAsync(id, cancellationToken);
        perfil.AlterarNome(request.Nome);
        await db.SaveChangesAsync(cancellationToken);
        return new PerfilResponse(perfil.Id, perfil.Nome, perfil.CodigoMoeda, perfil.Ativo, perfil.CriadoEm);
    }

    public async Task AtivarAsync(Guid id, CancellationToken cancellationToken)
    {
        var perfil = await ObterPerfilDoUsuarioAsync(id, cancellationToken);
        perfil.Ativar();
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DesativarAsync(Guid id, CancellationToken cancellationToken)
    {
        var perfil = await ObterPerfilDoUsuarioAsync(id, cancellationToken);
        perfil.Desativar();
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ValidarPerfilDoUsuarioAsync(Guid perfilId, CancellationToken cancellationToken)
    {
        var existe = await db.Perfis.AsNoTracking().AnyAsync(perfil => perfil.Id == perfilId && perfil.UsuarioId == usuarioAtual.UsuarioId, cancellationToken);
        if (!existe)
        {
            throw new NaoEncontradoException();
        }
    }

    private async Task<Perfil> ObterPerfilDoUsuarioAsync(Guid id, CancellationToken cancellationToken)
        => await db.Perfis.FirstOrDefaultAsync(perfil => perfil.Id == id && perfil.UsuarioId == usuarioAtual.UsuarioId, cancellationToken)
            ?? throw new NaoEncontradoException();
}
