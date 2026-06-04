using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Marketplace.Domain.Entities;

namespace Marketplace.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.OwnsOne(o => o.TotalAmount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("TotalAmount").HasColumnType("decimal(18,2)");
            money.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3);
        });

        builder.OwnsOne(o => o.ShippingAddress, address =>
        {
            address.Property(a => a.Country).HasMaxLength(100);
            address.Property(a => a.City).HasMaxLength(100);
            address.Property(a => a.Street).HasMaxLength(200);
            address.Property(a => a.Building).HasMaxLength(20);
            address.Property(a => a.Apartment).HasMaxLength(20);
            address.Property(a => a.ZipCode).HasMaxLength(20);
            address.Property(a => a.AdditionalInfo).HasMaxLength(500);
        });

        builder.Property(o => o.Status).HasConversion<string>().IsRequired();

        builder.HasMany(o => o.Items)
               .WithOne(i => i.Order)
               .HasForeignKey(i => i.OrderId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
