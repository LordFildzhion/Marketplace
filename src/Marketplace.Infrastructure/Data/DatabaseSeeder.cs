
using Marketplace.Domain.Entities;
using Marketplace.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Marketplace.Infrastructure.Data;

public class DatabaseSeeder
{
    private readonly AppDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(AppDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        if (await _context.Users.AnyAsync()) return;

        _logger.LogInformation("Seeding database...");

        var admin = new User(new Email("admin@marketplace.com"), BCrypt.Net.BCrypt.HashPassword("Admin123!"), "Admin", "User", Domain.Enums.UserRole.Admin);
        var seller = new User(new Email("seller@marketplace.com"), BCrypt.Net.BCrypt.HashPassword("Seller123!"), "Test", "Seller", Domain.Enums.UserRole.Seller);
        var customer = new User(new Email("customer@marketplace.com"), BCrypt.Net.BCrypt.HashPassword("Customer123!"), "Test", "Customer");

        await _context.Users.AddRangeAsync(admin, seller, customer);

        var electronics = new Category("Electronics", "Gadgets and devices");
        var clothing = new Category("Clothing", "Apparel and accessories");

        await _context.Categories.AddRangeAsync(electronics, clothing);
        await _context.SaveChangesAsync();

        var laptop = new Product(new Sku("LAP-001"), "Laptop Pro", "Powerful laptop", new Money(1299.99m, "USD"), 10, electronics.Id, seller.Id);
        var tshirt = new Product(new Sku("TSH-001"), "Cotton T-Shirt", "Comfortable cotton tee", new Money(29.99m, "USD"), 100, clothing.Id, seller.Id);

        await _context.Products.AddRangeAsync(laptop, tshirt);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Database seeded successfully.");
    }
}
