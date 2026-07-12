using ControleFinanceiro.Domain.Cartoes;
using ControleFinanceiro.Domain.Perfis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistencia.Configuracoes;

public sealed class FaturaCartaoConfiguracao : IEntityTypeConfiguration<FaturaCartao>
{
    public void Configure(EntityTypeBuilder<FaturaCartao> builder)
    {
        builder.ToTable("FaturasCartao");
        builder.HasKey(fatura => fatura.Id);
        builder.Property(fatura => fatura.Status).HasConversion<string>().IsRequired().HasMaxLength(20);
        builder.Property(fatura => fatura.CriadoEm).IsRequired();
        builder.HasOne<Perfil>().WithMany().HasForeignKey(fatura => fatura.PerfilId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CartaoCredito>().WithMany().HasForeignKey(fatura => fatura.CartaoCreditoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(fatura => new { fatura.CartaoCreditoId, fatura.MesReferencia }).IsUnique();
        builder.HasIndex(fatura => new { fatura.PerfilId, fatura.MesReferencia });
    }
}
