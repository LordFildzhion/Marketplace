
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Marketplace.Domain.Entities;

namespace Marketplace.Infrastructure.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(r => r.Id);
        builder.OwnsOne(r => r.Rating, rating =>
        {
            rating.Property(rt => rt.Value).HasColumnName("Rating").IsRequired();
        });
        builder.Property(r => r.Comment).IsRequired().HasMaxLength(2000);
        builder.HasOne(r => r.Product).WithMany(p => p.Reviews).HasForeignKey(r => r.ProductId);
        builder.HasOne(r => r.User).WithMany(u => u.Reviews).HasForeignKey(r => r.UserId);
    }
}
