using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Marketplace.Domain.Entities;

namespace Marketplace.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Sku)
               .HasConversion(sku => sku.Value, value => new Domain.ValueObjects.Sku(value))
               .IsRequired()
               .HasMaxLength(50);
        builder.HasIndex(p => p.Sku).IsUnique();

        builder.Property(p => p.Title).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).IsRequired().HasMaxLength(5000);

        builder.OwnsOne(p => p.Price, money =>
        {
            money.Property(m => m.Amount).HasColumnName("PriceAmount").HasColumnType("decimal(18,2)");
            money.Property(m => m.Currency).HasColumnName("PriceCurrency").HasMaxLength(3);
        });

        builder.OwnsOne(p => p.DiscountPrice, money =>
        {
            money.Property(m => m.Amount).HasColumnName("DiscountPriceAmount").HasColumnType("decimal(18,2)");
            money.Property(m => m.Currency).HasColumnName("DiscountPriceCurrency").HasMaxLength(3);
        });

        builder.Property(p => p.AttributesJson).HasColumnType("jsonb");

        builder.HasOne(p => p.Category)
               .WithMany(c => c.Products)
               .HasForeignKey(p => p.CategoryId)
               .IsRequired();
        builder.Navigation(p => p.Category).IsRequired(false); // разрешаем не загружать навигацию

        builder.HasOne(p => p.Seller)
               .WithMany()
               .HasForeignKey(p => p.SellerId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();
        builder.Navigation(p => p.Seller).IsRequired(false);

        builder.HasMany(p => p.Images).WithOne(i => i.Product).HasForeignKey(i => i.ProductId);
        builder.HasMany(p => p.Reviews).WithOne(r => r.Product).HasForeignKey(r => r.ProductId);
    }
}
