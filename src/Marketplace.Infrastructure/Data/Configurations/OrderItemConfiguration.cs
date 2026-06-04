
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Marketplace.Domain.Entities;

namespace Marketplace.Infrastructure.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.ProductTitle).IsRequired().HasMaxLength(200);
        builder.OwnsOne(i => i.UnitPrice, money =>
        {
            money.Property(m => m.Amount).HasColumnName("UnitPriceAmount").HasColumnType("decimal(18,2)");
            money.Property(m => m.Currency).HasColumnName("UnitPriceCurrency").HasMaxLength(3);
        });
        builder.HasOne(i => i.Order).WithMany(o => o.Items).HasForeignKey(i => i.OrderId);
    }
}
