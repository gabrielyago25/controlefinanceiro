using ControleFinanceiro.Domain.Cartoes;
using ControleFinanceiro.Domain.Perfis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistencia.Configuracoes;

public sealed class CompraCartaoConfiguracao : IEntityTypeConfiguration<CompraCartao>
{
    public void Configure(EntityTypeBuilder<CompraCartao> builder)
    {
        builder.ToTable("ComprasCartao");
        builder.HasKey(compra => compra.Id);
        builder.Property(compra => compra.Descricao).IsRequired().HasMaxLength(160);
        builder.Property(compra => compra.ValorTotal).IsRequired().HasColumnType("numeric(18,2)");
        builder.Property(compra => compra.CriadoEm).IsRequired();
        builder.HasOne<Perfil>().WithMany().HasForeignKey(compra => compra.PerfilId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CartaoCredito>().WithMany().HasForeignKey(compra => compra.CartaoCreditoId).OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(compra => compra.Parcelas).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(compra => new { compra.PerfilId, compra.CartaoCreditoId });
    }
}
