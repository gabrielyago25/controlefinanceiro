namespace ControleFinanceiro.Application.DTOs;

public sealed record CadastroRequest(string Nome, string Email, string Senha);
public sealed record LoginRequest(string Email, string Senha);
public sealed record UsuarioResponse(Guid Id, string Nome, string Email);
public sealed record AlterarUsuarioRequest(string Nome, string Email);
public sealed record AlterarSenhaRequest(string SenhaAtual, string NovaSenha);
public sealed record AutenticacaoResponse(string AccessToken, DateTime ExpiraEm, UsuarioResponse Usuario);
public sealed record AutenticacaoResultado(AutenticacaoResponse Resposta, string RefreshToken, DateTime RefreshTokenExpiraEm);
