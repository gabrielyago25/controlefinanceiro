using ControleFinanceiro.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguracao : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(usuario => usuario.Id);

        builder.Property(usuario => usuario.Nome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(usuario => usuario.Email)
            .IsRequired()
            .HasMaxLength(254);

        builder.Property(usuario => usuario.SenhaHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(usuario => usuario.Ativo)
            .IsRequired();

        builder.Property(usuario => usuario.CriadoEm)
            .IsRequired();

        builder.HasIndex(usuario => usuario.Email)
            .IsUnique();
    }
}