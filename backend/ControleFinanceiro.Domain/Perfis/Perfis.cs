namespace ControleFinanceiro.Domain.Perfis;

public class Perfil
{
    public Guid Id { get; private set; }

    public string Nome { get; private set; } = string.Empty;

    public Guid UsuarioId { get; private set; }

    public string CodigoMoeda { get; private set; } = string.Empty;

    public bool Ativo { get; private set; }

    public DateTime CriadoEm { get; private set; }

    private Perfil()
    {
    }

    public Perfil(
        string nome,
        Guid usuarioId,
        string codigoMoeda = "BRL")
    {
        ValidarNome(nome);
        ValidarUsuarioId(usuarioId);
        ValidarCodigoMoeda(codigoMoeda);

        Id = Guid.NewGuid();
        Nome = NormalizarNome(nome);
        UsuarioId = usuarioId;
        CodigoMoeda = NormalizarCodigoMoeda(codigoMoeda);
        Ativo = true;
        CriadoEm = DateTime.UtcNow;
    }

    public void AlterarNome(string novoNome)
    {
        ValidarNome(novoNome);

        Nome = NormalizarNome(novoNome);
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
            throw new ArgumentException(
                "O nome do perfil financeiro é obrigatório.",
                nameof(nome));
        }
    }

    private static void ValidarUsuarioId(Guid usuarioId)
    {
        if (usuarioId == Guid.Empty)
        {
            throw new ArgumentException(
                "O identificador do usuário é obrigatório.",
                nameof(usuarioId));
        }
    }

    private static void ValidarCodigoMoeda(string codigoMoeda)
    {
        if (string.IsNullOrWhiteSpace(codigoMoeda))
        {
            throw new ArgumentException(
                "O código da moeda é obrigatório.",
                nameof(codigoMoeda));
        }

        if (codigoMoeda.Trim().Length != 3)
        {
            throw new ArgumentException(
                "O código da moeda deve possuir exatamente 3 caracteres.",
                nameof(codigoMoeda));
        }
    }

    private static string NormalizarNome(string nome)
    {
        return nome.Trim();
    }

    private static string NormalizarCodigoMoeda(string codigoMoeda)
    {
        return codigoMoeda.Trim().ToUpperInvariant();
    }
}