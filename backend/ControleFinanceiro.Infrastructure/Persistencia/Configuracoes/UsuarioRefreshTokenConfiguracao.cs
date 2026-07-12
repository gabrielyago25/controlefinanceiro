using ControleFinanceiro.Domain.Autenticacao;
using ControleFinanceiro.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistencia.Configuracoes;

public sealed class UsuarioRefreshTokenConfiguracao : IEntityTypeConfiguration<UsuarioRefreshToken>
{
    public void Configure(EntityTypeBuilder<UsuarioRefreshToken> builder)
    {
        builder.ToTable("UsuarioRefreshTokens");
        builder.HasKey(token => token.Id);
        builder.Property(token => token.TokenHash).IsRequired().HasMaxLength(128);
        builder.Property(token => token.ExpiraEm).IsRequired();
        builder.Property(token => token.CriadoEm).IsRequired();
        builder.Property(token => token.SubstituidoPorTokenHash).HasMaxLength(128);
        builder.HasOne<Usuario>().WithMany().HasForeignKey(token => token.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(token => token.TokenHash).IsUnique();
        builder.HasIndex(token => token.UsuarioId);
    }
}
