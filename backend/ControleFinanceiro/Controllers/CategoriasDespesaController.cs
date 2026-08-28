using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Controllers;

[ApiController]
[Authorize]
[Route("api/categorias-despesa")]
public sealed class CategoriasDespesaController(FinanceiroServico financeiroServico, PerfisServico perfisServico) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoriaDespesaResponse>>> Listar(CancellationToken cancellationToken)
    {
        var perfilId = await perfisServico.ObterPerfilPadraoIdAsync(cancellationToken);
        return Ok(await financeiroServico.ListarCategoriasAsync(perfilId, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<CategoriaDespesaResponse>> Criar(SalvarCategoriaDespesaRequest request, CancellationToken cancellationToken)
    {
        var perfilId = await perfisServico.ObterPerfilPadraoIdAsync(cancellationToken);
        return Created(string.Empty, await financeiroServico.CriarCategoriaAsync(perfilId, request, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CategoriaDespesaResponse>> Alterar(Guid id, SalvarCategoriaDespesaRequest request, CancellationToken cancellationToken)
    {
        var perfilId = await perfisServico.ObterPerfilPadraoIdAsync(cancellationToken);
        return Ok(await financeiroServico.AlterarCategoriaAsync(perfilId, id, request, cancellationToken));
    }

    [HttpPatch("{id:guid}/ativar")]
    public async Task<IActionResult> Ativar(Guid id, CancellationToken cancellationToken)
    {
        var perfilId = await perfisServico.ObterPerfilPadraoIdAsync(cancellationToken);
        await financeiroServico.DefinirCategoriaAtivaAsync(perfilId, id, true, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/desativar")]
    public async Task<IActionResult> Desativar(Guid id, CancellationToken cancellationToken)
    {
        var perfilId = await perfisServico.ObterPerfilPadraoIdAsync(cancellationToken);
        await financeiroServico.DefinirCategoriaAtivaAsync(perfilId, id, false, cancellationToken);
        return NoContent();
    }
}
