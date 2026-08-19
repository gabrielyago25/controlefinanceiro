using ControleFinanceiro.Domain.Users;

namespace ControleFinanceiro.Application.Abstractions;

public interface ISenhaServico
{
    string GerarHash(Usuario usuario, string senha);
    bool Verificar(Usuario usuario, string hash, string senha);
}
