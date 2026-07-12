using ControleFinanceiro.Domain.Cartoes;
using ControleFinanceiro.Domain.Perfis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFinanceiro.Infrastructure.Persistencia.Configuracoes;

public sealed class CartaoCreditoConfiguracao : IEntityTypeConfiguration<CartaoCredito>
{
    public void Configure(EntityTypeBuilder<CartaoCredito> builder)
    {
        builder.ToTable("CartoesCredito");
        builder.HasKey(cartao => cartao.Id);
        builder.Property(cartao => cartao.Nome).IsRequired().HasMaxLength(100);
        builder.Property(cartao => cartao.Banco).IsRequired().HasMaxLength(100);
        builder.Property(cartao => cartao.Bandeira).IsRequired().HasMaxLength(50);
        builder.Property(cartao => cartao.Limite).IsRequired().HasColumnType("numeric(18,2)");
        builder.Property(cartao => cartao.Cor).HasMaxLength(30);
        builder.Property(cartao => cartao.CriadoEm).IsRequired();
        builder.HasOne<Perfil>().WithMany().HasForeignKey(cartao => cartao.PerfilId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(cartao => cartao.PerfilId);
    }
}
