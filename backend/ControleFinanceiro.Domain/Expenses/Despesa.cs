namespace ControleFinanceiro.Domain.Expenses;

public class Despesa
{
    public Guid Id { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public decimal Valor { get; private set; }
    public DateOnly DataVencimento { get; private set; }
    public DateOnly? DataPagamento { get; private set; }
    public DateOnly Competencia { get; private set; }
    public StatusDespesa Status { get; private set; }
    public string? Observacoes { get; private set; }
    public Guid CategoriaDespesaId { get; private set; }
    public Guid PerfilId { get; private set; }
    public DateTime CriadoEm { get; private set; }

    private Despesa() { }

    public Despesa(string descricao, decimal valor, DateOnly dataVencimento, DateOnly competencia, Guid categoriaDespesaId, Guid perfilId, string? observacoes = null)
    {
        Validar(descricao, valor, categoriaDespesaId, perfilId);

        Id = Guid.NewGuid();
        Descricao = descricao.Trim();
        Valor = decimal.Round(valor, 2);
        DataVencimento = dataVencimento;
        Competencia = NormalizarCompetencia(competencia);
        Status = StatusDespesa.Pendente;
        Observacoes = string.IsNullOrWhiteSpace(observacoes) ? null : observacoes.Trim();
        CategoriaDespesaId = categoriaDespesaId;
        PerfilId = perfilId;
        CriadoEm = DateTime.UtcNow;
    }

    public void Alterar(string descricao, decimal valor, DateOnly dataVencimento, DateOnly competencia, Guid categoriaDespesaId, string? observacoes)
    {
        Validar(descricao, valor, categoriaDespesaId, PerfilId);

        Descricao = descricao.Trim();
        Valor = decimal.Round(valor, 2);
        DataVencimento = dataVencimento;
        Competencia = NormalizarCompetencia(competencia);
        CategoriaDespesaId = categoriaDespesaId;
        Observacoes = string.IsNullOrWhiteSpace(observacoes) ? null : observacoes.Trim();
    }

    public void Pagar(DateOnly dataPagamento)
    {
        Status = StatusDespesa.Paga;
        DataPagamento = dataPagamento;
    }

    public void Reabrir()
    {
        Status = StatusDespesa.Pendente;
        DataPagamento = null;
    }

    public bool EstaAtrasada(DateOnly hoje) => Status == StatusDespesa.Pendente && DataVencimento < hoje;

    public static DateOnly NormalizarCompetencia(DateOnly competencia) => new(competencia.Year, competencia.Month, 1);

    private static void Validar(string descricao, decimal valor, Guid categoriaDespesaId, Guid perfilId)
    {
        if (string.IsNullOrWhiteSpace(descricao))
        {
            throw new ArgumentException("A descrição da despesa é obrigatória.", nameof(descricao));
        }

        if (valor <= 0)
        {
            throw new ArgumentException("O valor da despesa deve ser maior que zero.", nameof(valor));
        }

        if (categoriaDespesaId == Guid.Empty)
        {
            throw new ArgumentException("A categoria da despesa é obrigatória.", nameof(categoriaDespesaId));
        }

        if (perfilId == Guid.Empty)
        {
            throw new ArgumentException("O perfil é obrigatório.", nameof(perfilId));
        }
    }
}
