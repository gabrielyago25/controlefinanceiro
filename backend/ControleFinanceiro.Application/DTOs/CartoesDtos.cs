namespace ControleFinanceiro.Application.DTOs;

public sealed record CartaoCreditoResponse(Guid Id, string Nome, string Banco, string Bandeira, decimal Limite, int DiaFechamento, int DiaVencimento, string? Cor, bool Ativo);
public sealed record SalvarCartaoCreditoRequest(string Nome, string Banco, string Bandeira, decimal Limite, int DiaFechamento, int DiaVencimento, string? Cor);

public sealed record CompraCartaoResponse(Guid Id, string Descricao, decimal ValorTotal, DateOnly DataCompra, int QuantidadeParcelas, Guid CartaoCreditoId);
public sealed record CriarCompraCartaoRequest(string Descricao, decimal ValorTotal, DateOnly DataCompra, int QuantidadeParcelas);
public sealed record FaturaCartaoResponse(Guid Id, Guid CartaoCreditoId, DateOnly MesReferencia, DateOnly DataFechamento, DateOnly DataVencimento, string Status, decimal Valor);
