namespace ControleFinanceiro.Domain.Income;

public class Receita
{
    public Guid Id { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public decimal Valor { get; private set; }
    public DateOnly DataRecebimento { get; private set; }
    public DateOnly Competencia { get; private set; }
    public string? Observacoes { get; private set; }
    public Guid PerfilId { get; private set; }
    public DateTime CriadoEm { get; private set; }

    private Receita() { }

    public Receita(string descricao, decimal valor, DateOnly dataRecebimento, DateOnly competencia, Guid perfilId, string? observacoes = null)
    {
        Validar(descricao, valor, perfilId);

        Id = Guid.NewGuid();
        Descricao = descricao.Trim();
        Valor = decimal.Round(valor, 2);
        DataRecebimento = dataRecebimento;
        Competencia = new DateOnly(competencia.Year, competencia.Month, 1);
        Observacoes = string.IsNullOrWhiteSpace(observacoes) ? null : observacoes.Trim();
        PerfilId = perfilId;
        CriadoEm = DateTime.UtcNow;
    }

    public void Alterar(string descricao, decimal valor, DateOnly dataRecebimento, DateOnly competencia, string? observacoes)
    {
        Validar(descricao, valor, PerfilId);

        Descricao = descricao.Trim();
        Valor = decimal.Round(valor, 2);
        DataRecebimento = dataRecebimento;
        Competencia = new DateOnly(competencia.Year, competencia.Month, 1);
        Observacoes = string.IsNullOrWhiteSpace(observacoes) ? null : observacoes.Trim();
    }

    private static void Validar(string descricao, decimal valor, Guid perfilId)
    {
        if (string.IsNullOrWhiteSpace(descricao))
        {
            throw new ArgumentException("A descrição da receita é obrigatória.", nameof(descricao));
        }

        if (valor <= 0)
        {
            throw new ArgumentException("O valor da receita deve ser maior que zero.", nameof(valor));
        }

        if (perfilId == Guid.Empty)
        {
            throw new ArgumentException("O perfil é obrigatório.", nameof(perfilId));
        }
    }
}
