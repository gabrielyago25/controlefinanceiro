using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Controllers;

[ApiController]
[Authorize]
[Route("api/receitas")]
public sealed class ReceitasController(FinanceiroServico financeiroServico, PerfisServico perfisServico) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReceitaResponse>>> Listar([FromQuery] int mes, [FromQuery] int ano, CancellationToken cancellationToken)
    {
        var perfilId = await perfisServico.ObterPerfilPadraoIdAsync(cancellationToken);
        return Ok(await financeiroServico.ListarReceitasAsync(perfilId, mes, ano, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<ReceitaResponse>> Criar(SalvarReceitaRequest request, CancellationToken cancellationToken)
    {
        var perfilId = await perfisServico.ObterPerfilPadraoIdAsync(cancellationToken);
        return Created(string.Empty, await financeiroServico.CriarReceitaAsync(perfilId, request, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ReceitaResponse>> Alterar(Guid id, SalvarReceitaRequest request, CancellationToken cancellationToken)
    {
        var perfilId = await perfisServico.ObterPerfilPadraoIdAsync(cancellationToken);
        return Ok(await financeiroServico.AlterarReceitaAsync(perfilId, id, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken cancellationToken)
    {
        var perfilId = await perfisServico.ObterPerfilPadraoIdAsync(cancellationToken);
        await financeiroServico.ExcluirReceitaAsync(perfilId, id, cancellationToken);
        return NoContent();
    }
}
