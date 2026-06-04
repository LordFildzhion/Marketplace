
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Marketplace.Domain.Entities;

namespace Marketplace.Infrastructure.Data.Configurations;

public class ProductVariationConfiguration : IEntityTypeConfiguration<ProductVariation>
{
    public void Configure(EntityTypeBuilder<ProductVariation> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.VariationType).IsRequired().HasMaxLength(50);
        builder.Property(v => v.VariationValue).IsRequired().HasMaxLength(100);
        builder.OwnsOne(v => v.AdditionalPrice, money =>
        {
            money.Property(m => m.Amount).HasColumnName("AdditionalPriceAmount").HasColumnType("decimal(18,2)");
            money.Property(m => m.Currency).HasColumnName("AdditionalPriceCurrency").HasMaxLength(3);
        });
        builder.HasOne(v => v.Product).WithMany().HasForeignKey(v => v.ProductId);
    }
}
