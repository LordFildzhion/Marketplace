using FluentAssertions;
using Marketplace.Domain.Entities;
using Xunit;

namespace Marketplace.Domain.Tests.Entities;

public class CategoryTests
{
    [Fact]
    public void Constructor_ShouldGenerateSlug()
    {
        var category = new Category("Home & Garden");
        category.Slug.Should().Be("home-and-garden");
    }

    [Fact]
    public void Constructor_EmptyName_ShouldThrow()
    {
        Action act = () => new Category(" ");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Update_ShouldChangeNameDescriptionAndSlug()
    {
        var category = new Category("Old");
        category.Update("New Name", "Description");
        category.Name.Should().Be("New Name");
        category.Description.Should().Be("Description");
        category.Slug.Should().Be("new-name");
    }
}
