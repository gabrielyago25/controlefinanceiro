namespace ControleFinanceiro.Domain.CreditCards;

public static class ParcelamentoServico
{
    public static IReadOnlyList<decimal> Dividir(decimal valorTotal, int quantidadeParcelas)
    {
        if (valorTotal <= 0)
        {
            throw new ArgumentException("O valor total deve ser maior que zero.", nameof(valorTotal));
        }

        if (quantidadeParcelas is < 1 or > 24)
        {
            throw new ArgumentException("A quantidade de parcelas deve estar entre 1 e 24.", nameof(quantidadeParcelas));
        }

        var centavos = (long)decimal.Round(valorTotal * 100, 0, MidpointRounding.AwayFromZero);
        var baseCentavos = centavos / quantidadeParcelas;
        var resto = centavos % quantidadeParcelas;

        var parcelas = new List<decimal>(quantidadeParcelas);
        for (var indice = 1; indice <= quantidadeParcelas; indice++)
        {
            var valorParcelaCentavos = baseCentavos + (indice == quantidadeParcelas ? resto : 0);
            parcelas.Add(valorParcelaCentavos / 100m);
        }

        return parcelas;
    }
}
