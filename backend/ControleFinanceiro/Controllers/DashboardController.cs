using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public sealed class DashboardController(FinanceiroServico financeiroServico, PerfisServico perfisServico) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardResponse>> Obter([FromQuery] int mes, [FromQuery] int ano, CancellationToken cancellationToken)
    {
        var perfilId = await perfisServico.ObterPerfilPadraoIdAsync(cancellationToken);
        return Ok(await financeiroServico.ObterDashboardAsync(perfilId, mes, ano, cancellationToken));
    }
}
