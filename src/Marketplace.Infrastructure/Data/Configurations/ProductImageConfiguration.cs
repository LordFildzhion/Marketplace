
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Marketplace.Domain.Entities;

namespace Marketplace.Infrastructure.Data.Configurations;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Url).IsRequired().HasMaxLength(2048);
        builder.Property(i => i.SortOrder).HasDefaultValue(0);
        builder.HasOne(i => i.Product).WithMany(p => p.Images).HasForeignKey(i => i.ProductId);
    }
}
