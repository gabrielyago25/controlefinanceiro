using ControleFinanceiro.Domain.CreditCards;

namespace ControleFinanceiro.Domain.Tests.CreditCards;

public sealed class ParcelamentoServicoTests
{
    [Fact]
    public void Deve_dividir_valor_exato_em_centavos()
    {
        var parcelas = ParcelamentoServico.Dividir(100m, 3);

        Assert.Equal([33.33m, 33.33m, 33.34m], parcelas);
        Assert.Equal(100m, parcelas.Sum());
    }

    [Fact]
    public void Deve_dividir_compra_em_doze_parcelas_iguais()
    {
        var parcelas = ParcelamentoServico.Dividir(6000m, 12);

        Assert.All(parcelas, parcela => Assert.Equal(500m, parcela));
        Assert.Equal(6000m, parcelas.Sum());
    }

    [Fact]
    public void Nao_deve_permitir_mais_de_vinte_e_quatro_parcelas()
    {
        var excecao = Assert.Throws<ArgumentException>(() => ParcelamentoServico.Dividir(100m, 25));
        Assert.Contains("1 e 24", excecao.Message);
    }
}

public sealed class CartaoCreditoTests
{
    [Fact]
    public void Nao_deve_aceitar_bandeira_fora_das_opcoes_suportadas()
    {
        var excecao = Assert.Throws<ArgumentException>(() => new CartaoCredito("Cartão", "Banco", "Elo", 1000m, 10, 17, Guid.NewGuid()));
        Assert.Contains("Visa ou Mastercard", excecao.Message);
    }
}

public sealed class CompraCartaoTests
{
    [Fact]
    public void Nao_deve_criar_compra_com_mais_de_vinte_e_quatro_parcelas()
    {
        var excecao = Assert.Throws<ArgumentException>(() => new CompraCartao("Compra", 100m, new DateOnly(2026, 7, 12), 25, Guid.NewGuid(), Guid.NewGuid()));
        Assert.Contains("1 e 24", excecao.Message);
    }
}
