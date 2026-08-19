namespace ControleFinanceiro.Domain.Users;

public class Usuario
{
    public Guid Id { get; private set; }

    public string Nome { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string SenhaHash { get; private set; } = string.Empty;

    public bool Ativo { get; private set; }

    public DateTime CriadoEm { get; private set; }
    
    private Usuario()
    {
    }

    public Usuario(string nome, string email, string senhaHash)
    {
        ValidarNome(nome);
        ValidarEmail(email);
        ValidarSenhaHash(senhaHash);

        Id = Guid.NewGuid();
        Nome = NormalizarNome(nome);
        Email = NormalizarEmail(email);
        SenhaHash = senhaHash;
        Ativo = true;
        CriadoEm = DateTime.UtcNow;
    }

    public void AlterarNome(string novoNome)
    {
        ValidarNome(novoNome);
        Nome = NormalizarNome(novoNome);
    }
    public void AlterarEmail(string novoEmail)
    {
        ValidarEmail(novoEmail);
        Email = NormalizarEmail(novoEmail);
    }
    public void AlterarSenhaHash(string novaSenhaHash)
    {
        ValidarSenhaHash(novaSenhaHash);
        SenhaHash = novaSenhaHash;
    }
    public void Ativar()
    {
        Ativo = true;
    }
    public void Desativar()
    {
        Ativo = false;
    }

    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("O nome do usuário é obrigatório.", nameof(nome));
        }
    }

    private static void ValidarEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("O e-mail do usuário é obrigatório.", nameof(email));
        }
    }

    private static void ValidarSenhaHash(string senhaHash)
    {
        if (string.IsNullOrWhiteSpace(senhaHash))
        {
            throw new ArgumentException("O hash da senha é obrigatório.", nameof(senhaHash));
        }
    }

    private static string NormalizarNome(string nome)
    {
        return nome.Trim();
    }

    private static string NormalizarEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}