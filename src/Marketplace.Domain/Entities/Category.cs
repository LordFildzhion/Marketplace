
using Marketplace.Domain.Common;

namespace Marketplace.Domain.Entities;

public class Category : BaseEntity, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Slug { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public Category? ParentCategory { get; private set; }

    private readonly List<Category> _subCategories = new();
    public IReadOnlyCollection<Category> SubCategories => _subCategories.AsReadOnly();

    private readonly List<Product> _products = new();
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    public Category(string name, string? description = null, string? slug = null, Guid? parentCategoryId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        Name = name;
        Description = description ?? string.Empty;
        Slug = slug ?? GenerateSlug(name);
        ParentCategoryId = parentCategoryId;
    }

    public void Update(string name, string? description)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? string.Empty;
        Slug = GenerateSlug(name);
    }

    private static string GenerateSlug(string name) =>
        name.ToLowerInvariant().Replace(" ", "-").Replace("&", "and").Replace("--", "-");

    public void ClearDomainEvents() => _domainEvents.Clear();
    private Category() { } // for EF Core
}
