namespace ControleFinanceiro.Domain.CreditCards;

public class FaturaCartao
{
    public Guid Id { get; private set; }
    public Guid CartaoCreditoId { get; private set; }
    public Guid PerfilId { get; private set; }
    public DateOnly MesReferencia { get; private set; }
    public DateOnly DataFechamento { get; private set; }
    public DateOnly DataVencimento { get; private set; }
    public StatusFaturaCartao Status { get; private set; }
    public DateTime CriadoEm { get; private set; }

    private FaturaCartao() { }

    public FaturaCartao(Guid cartaoCreditoId, Guid perfilId, DateOnly mesReferencia, DateOnly dataFechamento, DateOnly dataVencimento)
    {
        if (cartaoCreditoId == Guid.Empty) throw new ArgumentException("O cartão é obrigatório.", nameof(cartaoCreditoId));
        if (perfilId == Guid.Empty) throw new ArgumentException("O perfil é obrigatório.", nameof(perfilId));

        Id = Guid.NewGuid();
        CartaoCreditoId = cartaoCreditoId;
        PerfilId = perfilId;
        MesReferencia = new DateOnly(mesReferencia.Year, mesReferencia.Month, 1);
        DataFechamento = dataFechamento;
        DataVencimento = dataVencimento;
        Status = StatusFaturaCartao.Aberta;
        CriadoEm = DateTime.UtcNow;
    }

    public void Pagar() => Status = StatusFaturaCartao.Paga;
}
