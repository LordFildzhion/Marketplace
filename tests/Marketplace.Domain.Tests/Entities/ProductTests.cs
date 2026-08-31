using FluentAssertions;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Events;
using Marketplace.Domain.ValueObjects;
using Xunit;

namespace Marketplace.Domain.Tests.Entities;

public class ProductTests
{
    private static Product Create() => new(new Sku("SKU"), "Phone", "Desc", new Money(100, "USD"), 5, Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public void Constructor_ShouldCreateProductEvent()
    {
        var product = Create();
        product.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<ProductCreatedEvent>();
    }

    [Fact]
    public void AddImage_ShouldAddImageWithSortOrder()
    {
        var product = Create();
        product.AddImage("a.jpg");
        product.AddImage("b.jpg");
        product.Images.Should().HaveCount(2);
        product.Images.Last().SortOrder.Should().Be(2);
    }

    [Fact]
    public void RemoveImage_ShouldRemoveExistingImage()
    {
        var product = Create();
        product.AddImage("a.jpg");
        var image = product.Images.Single();
        product.RemoveImage(image.Id);
        product.Images.Should().BeEmpty();
    }

    [Fact]
    public void ClearDomainEvents_ShouldClearEvents()
    {
        var product = Create();
        product.ClearDomainEvents();
        product.DomainEvents.Should().BeEmpty();
    }
}
