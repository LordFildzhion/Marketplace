using FluentAssertions;
using Marketplace.Application.Common.Exceptions;
using Marketplace.Application.DTOs.Categories;
using Marketplace.Application.Services;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Marketplace.Application.Tests.Services;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _repo = new();
    private readonly CategoryService _service;

    public CategoryServiceTests()
    {
        _service = new CategoryService(_repo.Object, Mock.Of<ILogger<CategoryService>>());
    }

    [Fact]
    public async Task GetAll_ShouldMapCategories()
    {
        var categories = new[] { new Category("Phones"), new Category("Books") };
        _repo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(categories);

        var result = await _service.GetAllAsync();

        result.Should().HaveCount(2);
        result[0].Slug.Should().Be("phones");
    }

    [Fact]
    public async Task GetAllWithSubcategories_ShouldReturnAllCategories()
    {
        var categories = new[] { new Category("Phones"), new Category("Books") };
        _repo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(categories);

        var result = await _service.GetAllWithSubcategoriesAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Create_ShouldPersistCategory()
    {
        var request = new CreateCategoryRequest { Name = "Home & Garden", Description = "Home" };
        _repo.Setup(r => r.AddAsync(It.IsAny<Category>(), default)).ReturnsAsync((Category c, CancellationToken _) => c);

        var result = await _service.CreateAsync(request);

        result.Name.Should().Be("Home & Garden");
        result.Slug.Should().Be("home-and-garden");
        _repo.Verify(r => r.AddAsync(It.IsAny<Category>(), default), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Update_NotFound_ShouldThrow()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Category?)null);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.UpdateAsync(Guid.NewGuid(), new CreateCategoryRequest { Name = "New" }));
    }

    [Fact]
    public async Task Delete_WithProducts_ShouldThrow()
    {
        var category = new Category("Phones");
        _repo.Setup(r => r.GetByIdAsync(category.Id, default)).ReturnsAsync(category);
        _repo.Setup(r => r.HasProductsAsync(category.Id, default)).ReturnsAsync(true);

        await Assert.ThrowsAsync<Marketplace.Application.Common.Exceptions.AppException>(() =>
            _service.DeleteAsync(category.Id));
        _repo.Verify(r => r.DeleteAsync(It.IsAny<Category>(), default), Times.Never);
    }

    [Fact]
    public async Task Delete_WithSubcategories_ShouldThrow()
    {
        var category = new Category("Phones");
        _repo.Setup(r => r.GetByIdAsync(category.Id, default)).ReturnsAsync(category);
        _repo.Setup(r => r.HasProductsAsync(category.Id, default)).ReturnsAsync(false);
        _repo.Setup(r => r.HasSubcategoriesAsync(category.Id, default)).ReturnsAsync(true);

        await Assert.ThrowsAsync<Marketplace.Application.Common.Exceptions.AppException>(() =>
            _service.DeleteAsync(category.Id));
    }

    [Fact]
    public async Task Delete_Valid_ShouldDeleteAndSave()
    {
        var category = new Category("Phones");
        _repo.Setup(r => r.GetByIdAsync(category.Id, default)).ReturnsAsync(category);
        _repo.Setup(r => r.HasProductsAsync(category.Id, default)).ReturnsAsync(false);
        _repo.Setup(r => r.HasSubcategoriesAsync(category.Id, default)).ReturnsAsync(false);

        await _service.DeleteAsync(category.Id);

        _repo.Verify(r => r.DeleteAsync(category, default), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }
}
