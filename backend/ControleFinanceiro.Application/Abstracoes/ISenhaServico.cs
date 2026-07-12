using ControleFinanceiro.Domain.Usuarios;

namespace ControleFinanceiro.Application.Abstracoes;

public interface ISenhaServico
{
    string GerarHash(Usuario usuario, string senha);
    bool Verificar(Usuario usuario, string hash, string senha);
}
