using ControleFinanceiro.Domain.Perfis;
using ControleFinanceiro.Domain.Receitas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistencia.Configuracoes;

public sealed class ReceitaConfiguracao : IEntityTypeConfiguration<Receita>
{
    public void Configure(EntityTypeBuilder<Receita> builder)
    {
        builder.ToTable("Receitas");
        builder.HasKey(receita => receita.Id);
        builder.Property(receita => receita.Descricao).IsRequired().HasMaxLength(160);
        builder.Property(receita => receita.Valor).IsRequired().HasColumnType("numeric(18,2)");
        builder.Property(receita => receita.Observacoes).HasMaxLength(500);
        builder.Property(receita => receita.CriadoEm).IsRequired();
        builder.HasOne<Perfil>().WithMany().HasForeignKey(receita => receita.PerfilId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(receita => new { receita.PerfilId, receita.Competencia });
    }
}
