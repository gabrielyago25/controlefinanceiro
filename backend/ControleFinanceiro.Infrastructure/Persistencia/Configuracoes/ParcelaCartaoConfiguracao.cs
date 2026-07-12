using ControleFinanceiro.Domain.Cartoes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistencia.Configuracoes;

public sealed class ParcelaCartaoConfiguracao : IEntityTypeConfiguration<ParcelaCartao>
{
    public void Configure(EntityTypeBuilder<ParcelaCartao> builder)
    {
        builder.ToTable("ParcelasCartao");
        builder.HasKey(parcela => parcela.Id);
        builder.Property(parcela => parcela.Valor).IsRequired().HasColumnType("numeric(18,2)");
        builder.HasOne<CompraCartao>().WithMany(compra => compra.Parcelas).HasForeignKey(parcela => parcela.CompraCartaoId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<FaturaCartao>().WithMany().HasForeignKey(parcela => parcela.FaturaCartaoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(parcela => parcela.FaturaCartaoId);
    }
}
