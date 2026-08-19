using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Controllers;

[ApiController]
[Route("api/autenticacao")]
public sealed class AutenticacaoController(AutenticacaoServico autenticacaoServico, IWebHostEnvironment environment) : ControllerBase
{
    private const string RefreshCookie = "controle_financeiro_refresh";

    [HttpPost("cadastro")]
    [AllowAnonymous]
    public async Task<ActionResult<AutenticacaoResponse>> Cadastrar(CadastroRequest request, CancellationToken cancellationToken)
    {
        var resultado = await autenticacaoServico.CadastrarAsync(request, cancellationToken);
        DefinirRefreshCookie(resultado, Response);
        return CreatedAtAction(nameof(Me), resultado.Resposta);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AutenticacaoResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var resultado = await autenticacaoServico.LoginAsync(request, cancellationToken);
        DefinirRefreshCookie(resultado, Response);
        return Ok(resultado.Resposta);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AutenticacaoResponse>> Refresh(CancellationToken cancellationToken)
    {
        Request.Cookies.TryGetValue(RefreshCookie, out var refreshToken);
        var resultado = await autenticacaoServico.RefreshAsync(refreshToken ?? string.Empty, cancellationToken);
        DefinirRefreshCookie(resultado, Response);
        return Ok(resultado.Resposta);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        Request.Cookies.TryGetValue(RefreshCookie, out var refreshToken);
        await autenticacaoServico.LogoutAsync(refreshToken ?? string.Empty, cancellationToken);
        Response.Cookies.Delete(RefreshCookie);
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UsuarioResponse>> Me(CancellationToken cancellationToken)
        => Ok(await autenticacaoServico.ObterUsuarioAtualAsync(cancellationToken));

    [HttpPut("me")]
    [Authorize]
    public async Task<ActionResult<UsuarioResponse>> AlterarMe(AlterarUsuarioRequest request, CancellationToken cancellationToken)
        => Ok(await autenticacaoServico.AlterarUsuarioAtualAsync(request, cancellationToken));

    [HttpPut("me/senha")]
    [Authorize]
    public async Task<IActionResult> AlterarSenha(AlterarSenhaRequest request, CancellationToken cancellationToken)
    {
        await autenticacaoServico.AlterarSenhaAsync(request, cancellationToken);
        return NoContent();
    }

    private void DefinirRefreshCookie(AutenticacaoResultado resposta, HttpResponse response)
    {
        response.Cookies.Append(RefreshCookie, resposta.RefreshToken, CriarCookieOptions(resposta.RefreshTokenExpiraEm));
    }

    private CookieOptions CriarCookieOptions(DateTime expiraEm)
        => new()
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment() && !environment.IsEnvironment("Testing"),
            SameSite = SameSiteMode.Strict,
            Expires = expiraEm
        };
}
