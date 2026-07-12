namespace ControleFinanceiro.Domain.Cartoes;

public class CartaoCredito
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Banco { get; private set; } = string.Empty;
    public string Bandeira { get; private set; } = string.Empty;
    public decimal Limite { get; private set; }
    public int DiaFechamento { get; private set; }
    public int DiaVencimento { get; private set; }
    public string? Cor { get; private set; }
    public bool Ativo { get; private set; }
    public Guid PerfilId { get; private set; }
    public DateTime CriadoEm { get; private set; }

    private CartaoCredito() { }

    public CartaoCredito(string nome, string banco, string bandeira, decimal limite, int diaFechamento, int diaVencimento, Guid perfilId, string? cor = null)
    {
        Validar(nome, banco, bandeira, limite, diaFechamento, diaVencimento, perfilId);

        Id = Guid.NewGuid();
        Nome = nome.Trim();
        Banco = banco.Trim();
        Bandeira = bandeira.Trim();
        Limite = decimal.Round(limite, 2);
        DiaFechamento = diaFechamento;
        DiaVencimento = diaVencimento;
        Cor = string.IsNullOrWhiteSpace(cor) ? null : cor.Trim();
        PerfilId = perfilId;
        Ativo = true;
        CriadoEm = DateTime.UtcNow;
    }

    public void Alterar(string nome, string banco, string bandeira, decimal limite, int diaFechamento, int diaVencimento, string? cor)
    {
        Validar(nome, banco, bandeira, limite, diaFechamento, diaVencimento, PerfilId);

        Nome = nome.Trim();
        Banco = banco.Trim();
        Bandeira = bandeira.Trim();
        Limite = decimal.Round(limite, 2);
        DiaFechamento = diaFechamento;
        DiaVencimento = diaVencimento;
        Cor = string.IsNullOrWhiteSpace(cor) ? null : cor.Trim();
    }

    public void Ativar() => Ativo = true;
    public void Desativar() => Ativo = false;

    private static void Validar(string nome, string banco, string bandeira, decimal limite, int diaFechamento, int diaVencimento, Guid perfilId)
    {
        if (string.IsNullOrWhiteSpace(nome)) throw new ArgumentException("O nome do cartão é obrigatório.", nameof(nome));
        if (string.IsNullOrWhiteSpace(banco)) throw new ArgumentException("O banco do cartão é obrigatório.", nameof(banco));
        if (!string.Equals(bandeira, "Visa", StringComparison.OrdinalIgnoreCase) && !string.Equals(bandeira, "Mastercard", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("A bandeira deve ser Visa ou Mastercard.", nameof(bandeira));
        if (limite < 0) throw new ArgumentException("O limite não pode ser negativo.", nameof(limite));
        if (diaFechamento is < 1 or > 31) throw new ArgumentException("O dia de fechamento deve estar entre 1 e 31.", nameof(diaFechamento));
        if (diaVencimento is < 1 or > 31) throw new ArgumentException("O dia de vencimento deve estar entre 1 e 31.", nameof(diaVencimento));
        if (perfilId == Guid.Empty) throw new ArgumentException("O perfil é obrigatório.", nameof(perfilId));
    }
}
