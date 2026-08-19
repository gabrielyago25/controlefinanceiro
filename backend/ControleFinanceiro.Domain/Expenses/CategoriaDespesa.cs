namespace ControleFinanceiro.Domain.Expenses;

public class CategoriaDespesa
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public Guid PerfilId { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime CriadoEm { get; private set; }

    private CategoriaDespesa() { }

    public CategoriaDespesa(string nome, Guid perfilId)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("O nome da categoria é obrigatório.", nameof(nome));
        }

        if (perfilId == Guid.Empty)
        {
            throw new ArgumentException("O perfil é obrigatório.", nameof(perfilId));
        }

        Id = Guid.NewGuid();
        Nome = nome.Trim();
        PerfilId = perfilId;
        Ativo = true;
        CriadoEm = DateTime.UtcNow;
    }

    public void AlterarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("O nome da categoria é obrigatório.", nameof(nome));
        }

        Nome = nome.Trim();
    }

    public void Ativar() => Ativo = true;
    public void Desativar() => Ativo = false;
}
