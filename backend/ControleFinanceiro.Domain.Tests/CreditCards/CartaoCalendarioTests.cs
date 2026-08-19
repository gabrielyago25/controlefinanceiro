using ControleFinanceiro.Domain.CreditCards;

namespace ControleFinanceiro.Domain.Tests.CreditCards;

public sealed class CartaoCalendarioTests
{
    [Fact]
    public void Deve_usar_ultimo_dia_valido_quando_dia_nao_existe_no_mes()
    {
        var data = CartaoCalendario.ObterDataValida(2027, 2, 31);

        Assert.Equal(new DateOnly(2027, 2, 28), data);
    }

    [Theory]
    [InlineData(2026, 7, 9, 2026, 7, 10)]
    [InlineData(2026, 7, 11, 2026, 8, 10)]
    public void Deve_calcular_fechamento_da_compra(int anoCompra, int mesCompra, int diaCompra, int anoFechamento, int mesFechamento, int diaFechamento)
    {
        var fechamento = CartaoCalendario.ObterFechamentoDaCompra(new DateOnly(anoCompra, mesCompra, diaCompra), 10);

        Assert.Equal(new DateOnly(anoFechamento, mesFechamento, diaFechamento), fechamento);
    }
}
