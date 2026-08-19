using ControleFinanceiro.Domain.Profiles;

namespace ControleFinanceiro.Domain.Tests.Profiles;

public sealed class PerfilTests
{
    [Fact]
    public void Nao_deve_criar_perfil_com_usuario_vazio()
    {
        Assert.Throws<ArgumentException>(() => new Perfil("Casa", Guid.Empty));
    }

    [Fact]
    public void Deve_normalizar_codigo_moeda()
    {
        var perfil = new Perfil("Casa", Guid.NewGuid(), " brl ");

        Assert.Equal("BRL", perfil.CodigoMoeda);
    }
}
