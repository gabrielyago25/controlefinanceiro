using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Controllers;

[ApiController]
[Authorize]
[Route("api/perfis/{perfilId:guid}/categorias-despesa")]
public sealed class CategoriasDespesaController(FinanceiroServico financeiroServico) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoriaDespesaResponse>>> Listar(Guid perfilId, CancellationToken cancellationToken)
        => Ok(await financeiroServico.ListarCategoriasAsync(perfilId, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<CategoriaDespesaResponse>> Criar(Guid perfilId, SalvarCategoriaDespesaRequest request, CancellationToken cancellationToken)
        => Created(string.Empty, await financeiroServico.CriarCategoriaAsync(perfilId, request, cancellationToken));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CategoriaDespesaResponse>> Alterar(Guid perfilId, Guid id, SalvarCategoriaDespesaRequest request, CancellationToken cancellationToken)
        => Ok(await financeiroServico.AlterarCategoriaAsync(perfilId, id, request, cancellationToken));

    [HttpPatch("{id:guid}/ativar")]
    public async Task<IActionResult> Ativar(Guid perfilId, Guid id, CancellationToken cancellationToken)
    {
        await financeiroServico.DefinirCategoriaAtivaAsync(perfilId, id, true, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/desativar")]
    public async Task<IActionResult> Desativar(Guid perfilId, Guid id, CancellationToken cancellationToken)
    {
        await financeiroServico.DefinirCategoriaAtivaAsync(perfilId, id, false, cancellationToken);
        return NoContent();
    }
}
