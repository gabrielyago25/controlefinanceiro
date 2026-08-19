namespace ControleFinanceiro.Application.Exceptions;

public abstract class AplicacaoException(string mensagem) : Exception(mensagem);

public sealed class ValidacaoException(string mensagem) : AplicacaoException(mensagem);

public sealed class NaoEncontradoException(string mensagem = "Recurso não encontrado.") : AplicacaoException(mensagem);

public sealed class ConflitoException(string mensagem) : AplicacaoException(mensagem);

public sealed class NaoAutenticadoException(string mensagem = "Usuário não autenticado.") : AplicacaoException(mensagem);
