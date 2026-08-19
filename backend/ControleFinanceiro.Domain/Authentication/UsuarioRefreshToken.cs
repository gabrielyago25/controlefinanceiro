namespace ControleFinanceiro.Domain.Authentication;

public class UsuarioRefreshToken
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiraEm { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime? RevogadoEm { get; private set; }
    public string? SubstituidoPorTokenHash { get; private set; }

    private UsuarioRefreshToken() { }

    public UsuarioRefreshToken(Guid usuarioId, string tokenHash, DateTime expiraEm)
    {
        if (usuarioId == Guid.Empty)
        {
            throw new ArgumentException("O usuário é obrigatório.", nameof(usuarioId));
        }

        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new ArgumentException("O hash do refresh token é obrigatório.", nameof(tokenHash));
        }

        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        TokenHash = tokenHash;
        ExpiraEm = expiraEm;
        CriadoEm = DateTime.UtcNow;
    }

    public bool EstaAtivo(DateTime agoraUtc) => RevogadoEm is null && ExpiraEm > agoraUtc;

    public void Revogar(string? substituidoPorTokenHash = null)
    {
        RevogadoEm = DateTime.UtcNow;
        SubstituidoPorTokenHash = substituidoPorTokenHash;
    }
}
