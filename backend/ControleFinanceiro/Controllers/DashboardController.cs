using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Controllers;

[ApiController]
[Authorize]
[Route("api/perfis/{perfilId:guid}/dashboard")]
public sealed class DashboardController(FinanceiroServico financeiroServico) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardResponse>> Obter(Guid perfilId, [FromQuery] int mes, [FromQuery] int ano, CancellationToken cancellationToken)
        => Ok(await financeiroServico.ObterDashboardAsync(perfilId, mes, ano, cancellationToken));
}
