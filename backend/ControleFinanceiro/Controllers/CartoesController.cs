using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Controllers;

[ApiController]
[Authorize]
[Route("api/cartoes")]
public sealed class CartoesController(CartoesServico cartoesServico, PerfisServico perfisServico) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CartaoCreditoResponse>>> Listar(CancellationToken cancellationToken)
    {
        var perfilId = await perfisServico.ObterPerfilPadraoIdAsync(cancellationToken);
        return Ok(await cartoesServico.ListarAsync(perfilId, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<CartaoCreditoResponse>> Criar(SalvarCartaoCreditoRequest request, CancellationToken cancellationToken)
    {
        var perfilId = await perfisServico.ObterPerfilPadraoIdAsync(cancellationToken);
        return Created(string.Empty, await cartoesServico.CriarAsync(perfilId, request, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CartaoCreditoResponse>> Alterar(Guid id, SalvarCartaoCreditoRequest request, CancellationToken cancellationToken)
    {
        var perfilId = await perfisServico.ObterPerfilPadraoIdAsync(cancellationToken);
        return Ok(await cartoesServico.AlterarAsync(perfilId, id, request, cancellationToken));
    }

    [HttpPatch("{id:guid}/ativar")]
    public async Task<IActionResult> Ativar(Guid id, CancellationToken cancellationToken)
    {
        var perfilId = await perfisServico.ObterPerfilPadraoIdAsync(cancellationToken);
        await cartoesServico.DefinirAtivoAsync(perfilId, id, true, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/desativar")]
    public async Task<IActionResult> Desativar(Guid id, CancellationToken cancellationToken)
    {
        var perfilId = await perfisServico.ObterPerfilPadraoIdAsync(cancellationToken);
        await cartoesServico.DefinirAtivoAsync(perfilId, id, false, cancellationToken);
        return NoContent();
    }

    [HttpGet("{cartaoId:guid}/compras")]
    public async Task<ActionResult<IReadOnlyList<CompraCartaoResponse>>> ListarCompras(Guid cartaoId, CancellationToken cancellationToken)
    {
        var perfilId = await perfisServico.ObterPerfilPadraoIdAsync(cancellationToken);
        return Ok(await cartoesServico.ListarComprasAsync(perfilId, cartaoId, cancellationToken));
    }

    [HttpPost("{cartaoId:guid}/compras")]
    public async Task<ActionResult<CompraCartaoResponse>> CriarCompra(Guid cartaoId, CriarCompraCartaoRequest request, CancellationToken cancellationToken)
    {
        var perfilId = await perfisServico.ObterPerfilPadraoIdAsync(cancellationToken);
        return Created(string.Empty, await cartoesServico.CriarCompraAsync(perfilId, cartaoId, request, cancellationToken));
    }

    [HttpGet("{cartaoId:guid}/faturas")]
    public async Task<ActionResult<IReadOnlyList<FaturaCartaoResponse>>> ListarFaturas(Guid cartaoId, CancellationToken cancellationToken)
    {
        var perfilId = await perfisServico.ObterPerfilPadraoIdAsync(cancellationToken);
        return Ok(await cartoesServico.ListarFaturasAsync(perfilId, cartaoId, cancellationToken));
    }

    [HttpPatch("{cartaoId:guid}/faturas/{faturaId:guid}/pagar")]
    public async Task<IActionResult> PagarFatura(Guid cartaoId, Guid faturaId, CancellationToken cancellationToken)
    {
        var perfilId = await perfisServico.ObterPerfilPadraoIdAsync(cancellationToken);
        await cartoesServico.PagarFaturaAsync(perfilId, cartaoId, faturaId, cancellationToken);
        return NoContent();
    }
}
