using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Controllers;

[ApiController]
[Authorize]
[Route("api/perfis")]
public sealed class PerfisController(PerfisServico perfisServico) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PerfilResponse>>> Listar(CancellationToken cancellationToken)
        => Ok(await perfisServico.ListarAsync(cancellationToken));

    [HttpPost]
    public async Task<ActionResult<PerfilResponse>> Criar(CriarPerfilRequest request, CancellationToken cancellationToken)
    {
        var perfil = await perfisServico.CriarAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Listar), new { id = perfil.Id }, perfil);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PerfilResponse>> Alterar(Guid id, AlterarPerfilRequest request, CancellationToken cancellationToken)
        => Ok(await perfisServico.AlterarAsync(id, request, cancellationToken));

    [HttpPatch("{id:guid}/ativar")]
    public async Task<IActionResult> Ativar(Guid id, CancellationToken cancellationToken)
    {
        await perfisServico.AtivarAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/desativar")]
    public async Task<IActionResult> Desativar(Guid id, CancellationToken cancellationToken)
    {
        await perfisServico.DesativarAsync(id, cancellationToken);
        return NoContent();
    }
}
