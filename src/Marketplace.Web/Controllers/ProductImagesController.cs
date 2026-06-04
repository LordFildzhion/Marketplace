using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Marketplace.Domain.Interfaces;
using Marketplace.Application.Interfaces;
using Marketplace.Application.Common.Exceptions;

namespace Marketplace.Web.Controllers;

[ApiController]
[Route("api/products/{productId:guid}/images")]
[Authorize(Roles = "Seller,Admin")]
public class ProductImagesController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IFileStorageService _fileStorage;

    public ProductImagesController(IProductService productService, IFileStorageService fileStorage)
    {
        _productService = productService;
        _fileStorage = fileStorage;
    }

    [HttpPost]
    public async Task<ActionResult> UploadImage(Guid productId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        using var stream = file.OpenReadStream();
        var url = await _fileStorage.UploadAsync(stream, file.FileName, file.ContentType);
        var imageId = await _productService.AddProductImageAsync(productId, url);
        return Ok(new { id = imageId, url });
    }

    [HttpDelete("{imageId:guid}")]
    public async Task<ActionResult> DeleteImage(Guid productId, Guid imageId)
    {
        await _productService.RemoveProductImageAsync(productId, imageId);
        return NoContent();
    }
}
