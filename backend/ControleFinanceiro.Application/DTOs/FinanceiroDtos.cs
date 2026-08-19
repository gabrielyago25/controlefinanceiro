namespace ControleFinanceiro.Application.DTOs;

public sealed record CategoriaDespesaResponse(Guid Id, string Nome, bool Ativo);
public sealed record SalvarCategoriaDespesaRequest(string Nome);

public sealed record DespesaResponse(Guid Id, string Descricao, decimal Valor, DateOnly DataVencimento, DateOnly? DataPagamento, DateOnly Competencia, string Status, bool Atrasada, string? Observacoes, Guid CategoriaDespesaId);
public sealed record SalvarDespesaRequest(string Descricao, decimal Valor, DateOnly DataVencimento, int Mes, int Ano, Guid CategoriaDespesaId, string? Observacoes);
public sealed record PagarDespesaRequest(DateOnly? DataPagamento);

public sealed record ReceitaResponse(Guid Id, string Descricao, decimal Valor, DateOnly DataRecebimento, DateOnly Competencia, string? Observacoes);
public sealed record SalvarReceitaRequest(string Descricao, decimal Valor, DateOnly DataRecebimento, int Mes, int Ano, string? Observacoes);

public sealed record DashboardResponse(
    decimal TotalReceitas,
    decimal TotalDespesas,
    decimal SaldoMensal,
    decimal TotalContasPagas,
    decimal TotalContasPendentes,
    decimal TotalContasAtrasadas,
    decimal ValorFaturasCartoes,
    IReadOnlyList<VencimentoResponse> ProximosVencimentos,
    IReadOnlyList<CategoriaResumoResponse> DistribuicaoDespesasPorCategoria,
    IReadOnlyList<EvolucaoMensalResponse> EvolucaoFinanceiraMensal);

public sealed record VencimentoResponse(Guid Id, string Descricao, DateOnly DataVencimento, decimal Valor);
public sealed record CategoriaResumoResponse(string Categoria, decimal Total);
public sealed record EvolucaoMensalResponse(DateOnly Competencia, decimal Receitas, decimal Despesas);
