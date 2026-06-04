
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Marketplace.Domain.Entities;

namespace Marketplace.Infrastructure.Data.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.HasKey(c => new { c.UserId, c.ProductId });
        builder.HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId);
        builder.HasOne(c => c.Product).WithMany().HasForeignKey(c => c.ProductId);
        builder.Property(c => c.AddedAt).IsRequired();
    }
}
