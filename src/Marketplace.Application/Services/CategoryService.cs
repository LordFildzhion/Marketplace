using Marketplace.Application.Common.Exceptions;
using Marketplace.Application.DTOs.Categories;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Marketplace.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(ICategoryRepository categoryRepository, ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<List<CategoryDto>> GetAllAsync(CancellationToken ct = default)
    {
        var categories = await _categoryRepository.GetAllAsync(ct);
        return categories.Select(c => new CategoryDto { Id = c.Id, Name = c.Name, Slug = c.Slug, ParentCategoryId = c.ParentCategoryId }).ToList();
    }

    public async Task<List<CategoryDto>> GetAllWithSubcategoriesAsync(CancellationToken ct = default)
    {
        var all = await _categoryRepository.GetAllAsync(ct);
        return all.Select(c => new CategoryDto { Id = c.Id, Name = c.Name, Slug = c.Slug, ParentCategoryId = c.ParentCategoryId }).ToList();
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default)
    {
        var category = new Category(request.Name, request.Description, null, request.ParentCategoryId);
        await _categoryRepository.AddAsync(category, ct);
        await _categoryRepository.SaveChangesAsync(ct);
        return new CategoryDto { Id = category.Id, Name = category.Name, Slug = category.Slug, ParentCategoryId = category.ParentCategoryId };
    }

    public async Task<CategoryDto> UpdateAsync(Guid id, CreateCategoryRequest request, CancellationToken ct = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, ct);
        if (category == null) throw new NotFoundException("Category", id);
        category.Update(request.Name, request.Description);
        await _categoryRepository.UpdateAsync(category, ct);
        await _categoryRepository.SaveChangesAsync(ct);
        return new CategoryDto { Id = category.Id, Name = category.Name, Slug = category.Slug, ParentCategoryId = category.ParentCategoryId };
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, ct);
        if (category == null) throw new NotFoundException("Category", id);

        if (await _categoryRepository.HasProductsAsync(id, ct))
            throw new AppException("Cannot delete category that has products.");
        if (await _categoryRepository.HasSubcategoriesAsync(id, ct))
            throw new AppException("Cannot delete category that has subcategories.");

        await _categoryRepository.DeleteAsync(category, ct);
        await _categoryRepository.SaveChangesAsync(ct);
    }
}
