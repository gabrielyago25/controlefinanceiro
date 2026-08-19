using ControleFinanceiro.Domain.Users;

namespace ControleFinanceiro.Domain.Tests.Users;

public sealed class UsuarioTests
{
    [Fact]
    public void Deve_normalizar_email()
    {
        var usuario = new Usuario("Gabriel", "  GABRIEL@EXEMPLO.COM  ", "hash-seguro");

        Assert.Equal("gabriel@exemplo.com", usuario.Email);
    }

    [Fact]
    public void Nao_deve_criar_usuario_sem_nome()
    {
        Assert.Throws<ArgumentException>(() => new Usuario("", "gabriel@exemplo.com", "hash-seguro"));
    }
}
