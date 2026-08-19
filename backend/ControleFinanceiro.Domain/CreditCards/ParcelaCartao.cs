namespace ControleFinanceiro.Domain.CreditCards;

public class ParcelaCartao
{
    public Guid Id { get; private set; }
    public Guid CompraCartaoId { get; private set; }
    public Guid FaturaCartaoId { get; private set; }
    public int NumeroParcela { get; private set; }
    public int QuantidadeParcelas { get; private set; }
    public decimal Valor { get; private set; }

    private ParcelaCartao() { }

    public ParcelaCartao(Guid compraCartaoId, Guid faturaCartaoId, int numeroParcela, int quantidadeParcelas, decimal valor)
    {
        if (compraCartaoId == Guid.Empty) throw new ArgumentException("A compra é obrigatória.", nameof(compraCartaoId));
        if (faturaCartaoId == Guid.Empty) throw new ArgumentException("A fatura é obrigatória.", nameof(faturaCartaoId));
        if (numeroParcela < 1 || numeroParcela > quantidadeParcelas) throw new ArgumentException("O número da parcela é inválido.", nameof(numeroParcela));
        if (valor <= 0) throw new ArgumentException("O valor da parcela deve ser maior que zero.", nameof(valor));

        Id = Guid.NewGuid();
        CompraCartaoId = compraCartaoId;
        FaturaCartaoId = faturaCartaoId;
        NumeroParcela = numeroParcela;
        QuantidadeParcelas = quantidadeParcelas;
        Valor = decimal.Round(valor, 2);
    }
}
