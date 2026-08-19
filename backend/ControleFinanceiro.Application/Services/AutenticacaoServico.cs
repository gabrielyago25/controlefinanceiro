using System.ComponentModel.DataAnnotations;
using ControleFinanceiro.Application.Abstractions;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Exceptions;
using ControleFinanceiro.Domain.Authentication;
using ControleFinanceiro.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Application.Services;

public sealed class AutenticacaoServico(IControleFinanceiroDbContext db, ISenhaServico senhaServico, ITokenServico tokenServico, IUsuarioAtualServico usuarioAtual)
{
    public async Task<AutenticacaoResultado> CadastrarAsync(CadastroRequest request, CancellationToken cancellationToken)
    {
        ValidarCadastro(request);

        var email = request.Email.Trim().ToLowerInvariant();
        var existe = await db.Usuarios.AsNoTracking().AnyAsync(usuario => usuario.Email == email, cancellationToken);
        if (existe)
        {
            throw new ConflitoException("Já existe um usuário com este e-mail.");
        }

        var usuario = new Usuario(request.Nome, email, "temporario");
        usuario.AlterarSenhaHash(senhaServico.GerarHash(usuario, request.Senha));

        var tokens = tokenServico.EmitirTokens(usuario);
        db.Usuarios.Add(usuario);
        db.UsuarioRefreshTokens.Add(new UsuarioRefreshToken(usuario.Id, tokenServico.GerarHashRefreshToken(tokens.RefreshToken), tokens.RefreshTokenExpiraEm));
        await db.SaveChangesAsync(cancellationToken);

        return Responder(usuario, tokens);
    }

    public async Task<AutenticacaoResultado> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Senha))
        {
            throw new NaoAutenticadoException("Credenciais inválidas.");
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        if (usuario is null || !usuario.Ativo || !senhaServico.Verificar(usuario, usuario.SenhaHash, request.Senha))
        {
            throw new NaoAutenticadoException("Credenciais inválidas.");
        }

        var tokens = tokenServico.EmitirTokens(usuario);
        db.UsuarioRefreshTokens.Add(new UsuarioRefreshToken(usuario.Id, tokenServico.GerarHashRefreshToken(tokens.RefreshToken), tokens.RefreshTokenExpiraEm));
        await db.SaveChangesAsync(cancellationToken);

        return Responder(usuario, tokens);
    }

    public async Task<AutenticacaoResultado> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new NaoAutenticadoException("Refresh token inválido.");
        }

        var tokenHash = tokenServico.GerarHashRefreshToken(refreshToken);
        var tokenSalvo = await db.UsuarioRefreshTokens.FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);
        if (tokenSalvo is null || !tokenSalvo.EstaAtivo(DateTime.UtcNow))
        {
            throw new NaoAutenticadoException("Refresh token inválido.");
        }

        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == tokenSalvo.UsuarioId, cancellationToken);
        if (usuario is null || !usuario.Ativo)
        {
            throw new NaoAutenticadoException("Refresh token inválido.");
        }

        var tokens = tokenServico.EmitirTokens(usuario);
        tokenSalvo.Revogar(tokenServico.GerarHashRefreshToken(tokens.RefreshToken));
        db.UsuarioRefreshTokens.Add(new UsuarioRefreshToken(usuario.Id, tokenServico.GerarHashRefreshToken(tokens.RefreshToken), tokens.RefreshTokenExpiraEm));
        await db.SaveChangesAsync(cancellationToken);

        return Responder(usuario, tokens);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        var tokenHash = tokenServico.GerarHashRefreshToken(refreshToken);
        var tokenSalvo = await db.UsuarioRefreshTokens.FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);
        if (tokenSalvo is not null && tokenSalvo.RevogadoEm is null)
        {
            tokenSalvo.Revogar();
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<UsuarioResponse> ObterUsuarioAtualAsync(CancellationToken cancellationToken)
    {
        var usuario = await db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == usuarioAtual.UsuarioId, cancellationToken)
            ?? throw new NaoEncontradoException();
        return new UsuarioResponse(usuario.Id, usuario.Nome, usuario.Email);
    }

    public async Task<UsuarioResponse> AlterarUsuarioAtualAsync(AlterarUsuarioRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Nome)) throw new ValidacaoException("Nome é obrigatório.");
        if (string.IsNullOrWhiteSpace(request.Email) || !new EmailAddressAttribute().IsValid(request.Email)) throw new ValidacaoException("E-mail inválido.");

        var usuario = await ObterEntidadeUsuarioAtualAsync(cancellationToken);
        var email = request.Email.Trim().ToLowerInvariant();
        var emailEmUso = await db.Usuarios.AsNoTracking().AnyAsync(u => u.Email == email && u.Id != usuario.Id, cancellationToken);
        if (emailEmUso) throw new ConflitoException("Já existe um usuário com este e-mail.");

        usuario.AlterarNome(request.Nome);
        usuario.AlterarEmail(email);
        await db.SaveChangesAsync(cancellationToken);
        return new UsuarioResponse(usuario.Id, usuario.Nome, usuario.Email);
    }

    public async Task AlterarSenhaAsync(AlterarSenhaRequest request, CancellationToken cancellationToken)
    {
        var usuario = await ObterEntidadeUsuarioAtualAsync(cancellationToken);
        if (!senhaServico.Verificar(usuario, usuario.SenhaHash, request.SenhaAtual))
            throw new NaoAutenticadoException("Senha atual inválida.");

        ValidarSenha(request.NovaSenha);
        usuario.AlterarSenhaHash(senhaServico.GerarHash(usuario, request.NovaSenha));
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Usuario> ObterEntidadeUsuarioAtualAsync(CancellationToken cancellationToken)
        => await db.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioAtual.UsuarioId, cancellationToken)
            ?? throw new NaoEncontradoException();

    private static AutenticacaoResultado Responder(Usuario usuario, TokenEmitido tokens)
        => new(
            new AutenticacaoResponse(tokens.AccessToken, tokens.ExpiraEm, new UsuarioResponse(usuario.Id, usuario.Nome, usuario.Email)),
            tokens.RefreshToken,
            tokens.RefreshTokenExpiraEm);

    private static void ValidarCadastro(CadastroRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome)) throw new ValidacaoException("Nome é obrigatório.");
        if (string.IsNullOrWhiteSpace(request.Email) || !new EmailAddressAttribute().IsValid(request.Email)) throw new ValidacaoException("E-mail inválido.");
        ValidarSenha(request.Senha);
    }

    private static void ValidarSenha(string senha)
    {
        if (string.IsNullOrWhiteSpace(senha) || senha.Length < 8) throw new ValidacaoException("A senha deve possuir ao menos 8 caracteres.");
        if (!senha.Any(char.IsUpper) || !senha.Any(char.IsLower) || !senha.Any(char.IsDigit))
        {
            throw new ValidacaoException("A senha deve conter letras maiúsculas, minúsculas e números.");
        }
    }
}
