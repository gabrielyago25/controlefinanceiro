using ControleFinanceiro.Domain.Profiles;
using ControleFinanceiro.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public class PerfisConfiguracao : IEntityTypeConfiguration<Perfil>
{
    public void Configure(EntityTypeBuilder<Perfil> builder)
    {
        builder.ToTable("Perfis");

        builder.HasKey(perfil => perfil.Id);

        builder.Property(perfil => perfil.Nome)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(perfil => perfil.UsuarioId)
            .IsRequired();

        builder.Property(perfil => perfil.CodigoMoeda)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(perfil => perfil.Ativo)
            .IsRequired();

        builder.Property(perfil => perfil.CriadoEm)
            .IsRequired();

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(perfil => perfil.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(perfil => perfil.UsuarioId);
    }
}