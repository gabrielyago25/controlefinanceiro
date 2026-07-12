using ControleFinanceiro.Domain.Despesas;
using ControleFinanceiro.Domain.Perfis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistencia.Configuracoes;

public sealed class CategoriaDespesaConfiguracao : IEntityTypeConfiguration<CategoriaDespesa>
{
    public void Configure(EntityTypeBuilder<CategoriaDespesa> builder)
    {
        builder.ToTable("CategoriasDespesa");
        builder.HasKey(categoria => categoria.Id);
        builder.Property(categoria => categoria.Nome).IsRequired().HasMaxLength(100);
        builder.Property(categoria => categoria.Ativo).IsRequired();
        builder.Property(categoria => categoria.CriadoEm).IsRequired();
        builder.HasOne<Perfil>().WithMany().HasForeignKey(categoria => categoria.PerfilId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(categoria => new { categoria.PerfilId, categoria.Nome }).IsUnique();
    }
}
