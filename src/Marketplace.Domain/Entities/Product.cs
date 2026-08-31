using Marketplace.Domain.Common;
using Marketplace.Domain.Events;
using Marketplace.Domain.ValueObjects;

namespace Marketplace.Domain.Entities;

public class Product : BaseEntity, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public Sku Sku { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public Money Price { get; private set; }
    public Money? DiscountPrice { get; private set; }
    public int Stock { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string AttributesJson { get; private set; } = "{}";
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; }
    public Guid SellerId { get; private set; }
    public User Seller { get; private set; }

    private readonly List<ProductImage> _images = new();
    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

    private readonly List<Review> _reviews = new();
    public IReadOnlyCollection<Review> Reviews => _reviews.AsReadOnly();

    private Product() { }

    public Product(Sku sku, string title, string description, Money price, int stock, Guid categoryId, Guid sellerId)
    {
        Sku = sku;
        Title = title;
        Description = description;
        Price = price;
        Stock = stock;
        CategoryId = categoryId;
        SellerId = sellerId;
        _domainEvents.Add(new ProductCreatedEvent(Id, sellerId, title));
    }

    public void UpdateInfo(string title, string description, Money? price = null)
    {
        Title = title;
        Description = description;
        if (price != null) Price = price;
    }

    public void ChangeCategory(Guid categoryId) => CategoryId = categoryId;
    public void ChangeStock(int newStock) => Stock = newStock;
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public Money CurrentPrice => DiscountPrice ?? Price;

    public void AddImage(string url, bool isMain = false)
    {
        var image = new ProductImage(url, isMain, _images.Count + 1);
        image.Product = this;
        _images.Add(image);
    }

    public void RemoveImage(Guid imageId)
    {
        var img = _images.FirstOrDefault(i => i.Id == imageId);
        if (img != null)
        {
            img.Product = null;
            _images.Remove(img);
        }
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}
