using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Controllers;

[ApiController]
[Authorize]
[Route("api/perfis/{perfilId:guid}/receitas")]
public sealed class ReceitasController(FinanceiroServico financeiroServico) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReceitaResponse>>> Listar(Guid perfilId, [FromQuery] int mes, [FromQuery] int ano, CancellationToken cancellationToken)
        => Ok(await financeiroServico.ListarReceitasAsync(perfilId, mes, ano, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ReceitaResponse>> Criar(Guid perfilId, SalvarReceitaRequest request, CancellationToken cancellationToken)
        => Created(string.Empty, await financeiroServico.CriarReceitaAsync(perfilId, request, cancellationToken));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ReceitaResponse>> Alterar(Guid perfilId, Guid id, SalvarReceitaRequest request, CancellationToken cancellationToken)
        => Ok(await financeiroServico.AlterarReceitaAsync(perfilId, id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Excluir(Guid perfilId, Guid id, CancellationToken cancellationToken)
    {
        await financeiroServico.ExcluirReceitaAsync(perfilId, id, cancellationToken);
        return NoContent();
    }
}
