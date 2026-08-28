using ControleFinanceiro.Application.Abstractions;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Exceptions;
using ControleFinanceiro.Domain.CreditCards;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Application.Services;

public sealed class CartoesServico(IControleFinanceiroDbContext db, PerfisServico perfisServico)
{
    public async Task<IReadOnlyList<CartaoCreditoResponse>> ListarAsync(Guid perfilId, int mes, int ano, CancellationToken cancellationToken)
    {
        await perfisServico.ValidarPerfilDoUsuarioAsync(perfilId, cancellationToken);
        var competencia = ObterCompetencia(mes, ano);

        return await db.CartoesCredito.AsNoTracking()
            .Where(cartao => cartao.PerfilId == perfilId)
            .OrderBy(cartao => cartao.Nome)
            .Select(cartao => new CartaoCreditoResponse(
                cartao.Id,
                cartao.Nome,
                cartao.Banco,
                cartao.Bandeira,
                cartao.Limite,
                db.ComprasCartao
                    .Where(compra => compra.CartaoCreditoId == cartao.Id
                        && db.ParcelasCartao.Any(parcela => parcela.CompraCartaoId == compra.Id
                            && parcela.NumeroParcela == 1
                            && db.FaturasCartao.Any(fatura => fatura.Id == parcela.FaturaCartaoId
                                && fatura.MesReferencia == competencia)))
                    .Sum(compra => (decimal?)compra.ValorTotal) ?? 0,
                cartao.DiaFechamento,
                cartao.DiaVencimento,
                cartao.Cor,
                cartao.Ativo))
            .ToListAsync(cancellationToken);
    }

    public async Task<CartaoCreditoResponse> CriarAsync(Guid perfilId, SalvarCartaoCreditoRequest request, CancellationToken cancellationToken)
    {
        await perfisServico.ValidarPerfilDoUsuarioAsync(perfilId, cancellationToken);
        var cartao = new CartaoCredito(request.Nome, request.Banco, request.Bandeira, request.Limite, request.DiaFechamento, request.DiaVencimento, perfilId, request.Cor);
        db.CartoesCredito.Add(cartao);
        await db.SaveChangesAsync(cancellationToken);
        return await MapearCartaoAsync(cartao, cancellationToken);
    }

    public async Task<CartaoCreditoResponse> AlterarAsync(Guid perfilId, Guid id, SalvarCartaoCreditoRequest request, CancellationToken cancellationToken)
    {
        var cartao = await ObterCartaoAsync(perfilId, id, cancellationToken);
        cartao.Alterar(request.Nome, request.Banco, request.Bandeira, request.Limite, request.DiaFechamento, request.DiaVencimento, request.Cor);
        await db.SaveChangesAsync(cancellationToken);
        return await MapearCartaoAsync(cartao, cancellationToken);
    }

    public async Task DefinirAtivoAsync(Guid perfilId, Guid id, bool ativo, CancellationToken cancellationToken)
    {
        var cartao = await ObterCartaoAsync(perfilId, id, cancellationToken);
        if (ativo) cartao.Ativar(); else cartao.Desativar();
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<CompraCartaoResponse> CriarCompraAsync(Guid perfilId, Guid cartaoId, CriarCompraCartaoRequest request, CancellationToken cancellationToken)
    {
        var cartao = await ObterCartaoAsync(perfilId, cartaoId, cancellationToken);
        if (!cartao.Ativo)
        {
            throw new ValidacaoException("Não é possível lançar compra em cartão desativado.");
        }

        await using var transaction = await db.BeginTransactionAsync(cancellationToken);

        var compra = new CompraCartao(request.Descricao, request.ValorTotal, request.DataCompra, request.QuantidadeParcelas, cartao.Id, perfilId);
        db.ComprasCartao.Add(compra);

        var valores = ParcelamentoServico.Dividir(compra.ValorTotal, compra.QuantidadeParcelas);
        var primeiraDataFechamento = CartaoCalendario.ObterFechamentoDaCompra(compra.DataCompra, cartao.DiaFechamento);

        for (var indice = 0; indice < valores.Count; indice++)
        {
            var dataFechamento = primeiraDataFechamento.AddMonths(indice);
            var fatura = await ObterOuCriarFaturaAsync(cartao, perfilId, dataFechamento, cancellationToken);
            var parcela = new ParcelaCartao(compra.Id, fatura.Id, indice + 1, compra.QuantidadeParcelas, valores[indice]);
            compra.AdicionarParcela(parcela);
            db.ParcelasCartao.Add(parcela);
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new CompraCartaoResponse(compra.Id, compra.Descricao, compra.ValorTotal, valores[0], compra.DataCompra, 1, compra.QuantidadeParcelas, compra.CartaoCreditoId);
    }

    public async Task<IReadOnlyList<CompraCartaoResponse>> ListarComprasAsync(Guid perfilId, Guid cartaoId, int mes, int ano, CancellationToken cancellationToken)
    {
        await ObterCartaoAsync(perfilId, cartaoId, cancellationToken);
        var competencia = ObterCompetencia(mes, ano);
        var compras = await db.ComprasCartao.AsNoTracking()
            .Where(compra => compra.PerfilId == perfilId
                && compra.CartaoCreditoId == cartaoId
                && db.ParcelasCartao.Any(parcela => parcela.CompraCartaoId == compra.Id
                    && parcela.NumeroParcela == 1
                    && db.FaturasCartao.Any(fatura => fatura.Id == parcela.FaturaCartaoId
                        && fatura.MesReferencia == competencia)))
            .OrderByDescending(compra => compra.DataCompra)
            .ToListAsync(cancellationToken);

        var compraIds = compras.Select(compra => compra.Id).ToArray();
        var parcelas = await (
            from parcela in db.ParcelasCartao.AsNoTracking()
            join fatura in db.FaturasCartao.AsNoTracking() on parcela.FaturaCartaoId equals fatura.Id
            where compraIds.Contains(parcela.CompraCartaoId)
            select new { parcela.CompraCartaoId, parcela.NumeroParcela, parcela.Valor, fatura.MesReferencia })
            .ToListAsync(cancellationToken);

        var parcelasPorCompra = parcelas
            .GroupBy(parcela => parcela.CompraCartaoId)
            .ToDictionary(grupo => grupo.Key, grupo => grupo.OrderBy(parcela => parcela.NumeroParcela).ToList());

        return compras.Select(compra =>
        {
            var parcelasDaCompra = parcelasPorCompra.GetValueOrDefault(compra.Id) ?? [];
            var parcelaAtual = parcelasDaCompra.FirstOrDefault(parcela => parcela.MesReferencia == competencia);

            return new CompraCartaoResponse(
                compra.Id,
                compra.Descricao,
                compra.ValorTotal,
                parcelaAtual?.Valor ?? ParcelamentoServico.Dividir(compra.ValorTotal, compra.QuantidadeParcelas)[0],
                compra.DataCompra,
                parcelaAtual?.NumeroParcela ?? 1,
                compra.QuantidadeParcelas,
                compra.CartaoCreditoId);
        }).ToList();
    }

    public async Task<IReadOnlyList<FaturaCartaoResponse>> ListarFaturasAsync(Guid perfilId, Guid cartaoId, CancellationToken cancellationToken)
    {
        await ObterCartaoAsync(perfilId, cartaoId, cancellationToken);
        var faturas = await db.FaturasCartao.AsNoTracking()
            .Where(fatura => fatura.PerfilId == perfilId && fatura.CartaoCreditoId == cartaoId)
            .OrderByDescending(fatura => fatura.MesReferencia)
            .ToListAsync(cancellationToken);

        var faturaIds = faturas.Select(f => f.Id).ToArray();
        var parcelas = await db.ParcelasCartao.AsNoTracking()
            .Where(parcela => faturaIds.Contains(parcela.FaturaCartaoId))
            .GroupBy(parcela => parcela.FaturaCartaoId)
            .Select(grupo => new { FaturaCartaoId = grupo.Key, Valor = grupo.Sum(parcela => parcela.Valor) })
            .ToDictionaryAsync(item => item.FaturaCartaoId, item => item.Valor, cancellationToken);

        return faturas.Select(fatura => new FaturaCartaoResponse(
            fatura.Id,
            fatura.CartaoCreditoId,
            fatura.MesReferencia,
            fatura.DataFechamento,
            fatura.DataVencimento,
            fatura.Status.ToString(),
            parcelas.GetValueOrDefault(fatura.Id))).ToList();
    }

    public async Task PagarFaturaAsync(Guid perfilId, Guid cartaoId, Guid faturaId, CancellationToken cancellationToken)
    {
        await ObterCartaoAsync(perfilId, cartaoId, cancellationToken);
        var fatura = await db.FaturasCartao.FirstOrDefaultAsync(f => f.Id == faturaId && f.CartaoCreditoId == cartaoId && f.PerfilId == perfilId, cancellationToken)
            ?? throw new NaoEncontradoException();
        fatura.Pagar();
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<CartaoCredito> ObterCartaoAsync(Guid perfilId, Guid cartaoId, CancellationToken cancellationToken)
    {
        await perfisServico.ValidarPerfilDoUsuarioAsync(perfilId, cancellationToken);
        return await db.CartoesCredito.FirstOrDefaultAsync(cartao => cartao.Id == cartaoId && cartao.PerfilId == perfilId, cancellationToken)
            ?? throw new NaoEncontradoException();
    }

    private async Task<FaturaCartao> ObterOuCriarFaturaAsync(CartaoCredito cartao, Guid perfilId, DateOnly dataFechamento, CancellationToken cancellationToken)
    {
        var mesReferencia = new DateOnly(dataFechamento.Year, dataFechamento.Month, 1);
        var fatura = await db.FaturasCartao.FirstOrDefaultAsync(f => f.CartaoCreditoId == cartao.Id && f.MesReferencia == mesReferencia, cancellationToken);
        if (fatura is not null)
        {
            if (fatura.Status != StatusFaturaCartao.Aberta)
            {
                throw new ValidacaoException("Não é possível alterar fatura paga ou bloqueada.");
            }

            return fatura;
        }

        var dataVencimento = CartaoCalendario.ObterVencimento(dataFechamento, cartao.DiaVencimento);
        fatura = new FaturaCartao(cartao.Id, perfilId, mesReferencia, dataFechamento, dataVencimento);
        db.FaturasCartao.Add(fatura);
        return fatura;
    }

    private async Task<CartaoCreditoResponse> MapearCartaoAsync(CartaoCredito cartao, CancellationToken cancellationToken)
    {
        var limiteUtilizado = await db.ComprasCartao.AsNoTracking()
            .Where(compra => compra.CartaoCreditoId == cartao.Id)
            .SumAsync(compra => (decimal?)compra.ValorTotal, cancellationToken) ?? 0;

        return new CartaoCreditoResponse(cartao.Id, cartao.Nome, cartao.Banco, cartao.Bandeira, cartao.Limite, limiteUtilizado, cartao.DiaFechamento, cartao.DiaVencimento, cartao.Cor, cartao.Ativo);
    }

    private static DateOnly ObterCompetencia(int mes, int ano)
    {
        if (mes is < 1 or > 12) throw new ValidacaoException("Mês inválido.");
        if (ano < 1900) throw new ValidacaoException("Ano inválido.");
        return new DateOnly(ano, mes, 1);
    }
}
