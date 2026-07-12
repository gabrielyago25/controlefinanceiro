using ControleFinanceiro.Application.Dtos;
using ControleFinanceiro.Application.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Controllers;

[ApiController]
[Authorize]
[Route("api/perfis/{perfilId:guid}/cartoes")]
public sealed class CartoesController(CartoesServico cartoesServico) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CartaoCreditoResponse>>> Listar(Guid perfilId, CancellationToken cancellationToken)
        => Ok(await cartoesServico.ListarAsync(perfilId, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<CartaoCreditoResponse>> Criar(Guid perfilId, SalvarCartaoCreditoRequest request, CancellationToken cancellationToken)
        => Created(string.Empty, await cartoesServico.CriarAsync(perfilId, request, cancellationToken));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CartaoCreditoResponse>> Alterar(Guid perfilId, Guid id, SalvarCartaoCreditoRequest request, CancellationToken cancellationToken)
        => Ok(await cartoesServico.AlterarAsync(perfilId, id, request, cancellationToken));

    [HttpPatch("{id:guid}/ativar")]
    public async Task<IActionResult> Ativar(Guid perfilId, Guid id, CancellationToken cancellationToken)
    {
        await cartoesServico.DefinirAtivoAsync(perfilId, id, true, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/desativar")]
    public async Task<IActionResult> Desativar(Guid perfilId, Guid id, CancellationToken cancellationToken)
    {
        await cartoesServico.DefinirAtivoAsync(perfilId, id, false, cancellationToken);
        return NoContent();
    }

    [HttpGet("{cartaoId:guid}/compras")]
    public async Task<ActionResult<IReadOnlyList<CompraCartaoResponse>>> ListarCompras(Guid perfilId, Guid cartaoId, CancellationToken cancellationToken)
        => Ok(await cartoesServico.ListarComprasAsync(perfilId, cartaoId, cancellationToken));

    [HttpPost("{cartaoId:guid}/compras")]
    public async Task<ActionResult<CompraCartaoResponse>> CriarCompra(Guid perfilId, Guid cartaoId, CriarCompraCartaoRequest request, CancellationToken cancellationToken)
        => Created(string.Empty, await cartoesServico.CriarCompraAsync(perfilId, cartaoId, request, cancellationToken));

    [HttpGet("{cartaoId:guid}/faturas")]
    public async Task<ActionResult<IReadOnlyList<FaturaCartaoResponse>>> ListarFaturas(Guid perfilId, Guid cartaoId, CancellationToken cancellationToken)
        => Ok(await cartoesServico.ListarFaturasAsync(perfilId, cartaoId, cancellationToken));

    [HttpPatch("{cartaoId:guid}/faturas/{faturaId:guid}/pagar")]
    public async Task<IActionResult> PagarFatura(Guid perfilId, Guid cartaoId, Guid faturaId, CancellationToken cancellationToken)
    {
        await cartoesServico.PagarFaturaAsync(perfilId, cartaoId, faturaId, cancellationToken);
        return NoContent();
    }
}
