namespace ControleFinanceiro.Domain.CreditCards;

public class CompraCartao
{
    private readonly List<ParcelaCartao> _parcelas = [];

    public Guid Id { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public decimal ValorTotal { get; private set; }
    public DateOnly DataCompra { get; private set; }
    public int QuantidadeParcelas { get; private set; }
    public Guid CartaoCreditoId { get; private set; }
    public Guid PerfilId { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public IReadOnlyCollection<ParcelaCartao> Parcelas => _parcelas;

    private CompraCartao() { }

    public CompraCartao(string descricao, decimal valorTotal, DateOnly dataCompra, int quantidadeParcelas, Guid cartaoCreditoId, Guid perfilId)
    {
        if (string.IsNullOrWhiteSpace(descricao)) throw new ArgumentException("A descrição da compra é obrigatória.", nameof(descricao));
        if (valorTotal <= 0) throw new ArgumentException("O valor da compra deve ser maior que zero.", nameof(valorTotal));
        if (quantidadeParcelas is < 1 or > 24) throw new ArgumentException("A compra deve possuir entre 1 e 24 parcelas.", nameof(quantidadeParcelas));
        if (cartaoCreditoId == Guid.Empty) throw new ArgumentException("O cartão é obrigatório.", nameof(cartaoCreditoId));
        if (perfilId == Guid.Empty) throw new ArgumentException("O perfil é obrigatório.", nameof(perfilId));

        Id = Guid.NewGuid();
        Descricao = descricao.Trim();
        ValorTotal = decimal.Round(valorTotal, 2);
        DataCompra = dataCompra;
        QuantidadeParcelas = quantidadeParcelas;
        CartaoCreditoId = cartaoCreditoId;
        PerfilId = perfilId;
        CriadoEm = DateTime.UtcNow;
    }

    public void AdicionarParcela(ParcelaCartao parcela)
    {
        if (parcela.CompraCartaoId != Id)
        {
            throw new InvalidOperationException("A parcela não pertence à compra.");
        }

        _parcelas.Add(parcela);
    }
}
