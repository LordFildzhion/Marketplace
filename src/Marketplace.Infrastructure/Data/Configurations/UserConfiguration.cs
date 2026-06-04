using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Marketplace.Domain.Entities;
using Marketplace.Domain.ValueObjects;

namespace Marketplace.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.Email)
            .HasConversion(
                email => email.Value,               // в БД
                value => new Email(value))          // из БД
            .IsRequired()
            .HasMaxLength(256);
        
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Phone).HasMaxLength(20);
        builder.Property(u => u.Role).HasConversion<string>().IsRequired();
        
        builder.HasMany(u => u.Orders).WithOne(o => o.User).HasForeignKey(o => o.UserId);
        builder.HasMany(u => u.Reviews).WithOne(r => r.User).HasForeignKey(r => r.UserId);
        builder.HasMany(u => u.RefreshTokens).WithOne(rt => rt.User).HasForeignKey(rt => rt.UserId);
    }
}
