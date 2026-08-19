using ControleFinanceiro.Domain.Expenses;
using ControleFinanceiro.Domain.Profiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistence.Configurations;

public sealed class DespesaConfiguracao : IEntityTypeConfiguration<Despesa>
{
    public void Configure(EntityTypeBuilder<Despesa> builder)
    {
        builder.ToTable("Despesas");
        builder.HasKey(despesa => despesa.Id);
        builder.Property(despesa => despesa.Descricao).IsRequired().HasMaxLength(160);
        builder.Property(despesa => despesa.Valor).IsRequired().HasColumnType("numeric(18,2)");
        builder.Property(despesa => despesa.Status).HasConversion<string>().IsRequired().HasMaxLength(20);
        builder.Property(despesa => despesa.Observacoes).HasMaxLength(500);
        builder.Property(despesa => despesa.CriadoEm).IsRequired();
        builder.HasOne<Perfil>().WithMany().HasForeignKey(despesa => despesa.PerfilId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CategoriaDespesa>().WithMany().HasForeignKey(despesa => despesa.CategoriaDespesaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(despesa => new { despesa.PerfilId, despesa.Competencia });
        builder.HasIndex(despesa => despesa.CategoriaDespesaId);
    }
}
