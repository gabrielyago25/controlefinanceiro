using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Controllers;

[ApiController]
[Authorize]
[Route("api/despesas")]
public sealed class DespesasController(FinanceiroServico financeiroServico, PerfisServico perfisServico) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DespesaResponse>>> Listar([FromQuery] int mes, [FromQuery] int ano, CancellationToken cancellationToken)
    {
        var perfilId = await perfisServico.ObterPerfilPadraoIdAsync(cancellationToken);
        return Ok(await financeiroServico.ListarDespesasAsync(perfilId, mes, ano, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<DespesaResponse>> Criar(SalvarDespesaRequest request, CancellationToken cancellationToken)
    {
        var perfilId = await perfisServico.ObterPerfilPadraoIdAsync(cancellationToken);
        return Created(string.Empty, await financeiroServico.CriarDespesaAsync(perfilId, request, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DespesaResponse>> Alterar(Guid id, SalvarDespesaRequest request, CancellationToken cancellationToken)
    {
        var perfilId = await perfisServico.ObterPerfilPadraoIdAsync(cancellationToken);
        return Ok(await financeiroServico.AlterarDespesaAsync(perfilId, id, request, cancellationToken));
    }

    [HttpPatch("{id:guid}/pagar")]
    public async Task<IActionResult> Pagar(Guid id, PagarDespesaRequest request, CancellationToken cancellationToken)
    {
        var perfilId = await perfisServico.ObterPerfilPadraoIdAsync(cancellationToken);
        await financeiroServico.PagarDespesaAsync(perfilId, id, request, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/reabrir")]
    public async Task<IActionResult> Reabrir(Guid id, CancellationToken cancellationToken)
    {
        var perfilId = await perfisServico.ObterPerfilPadraoIdAsync(cancellationToken);
        await financeiroServico.ReabrirDespesaAsync(perfilId, id, cancellationToken);
        return NoContent();
    }
}
