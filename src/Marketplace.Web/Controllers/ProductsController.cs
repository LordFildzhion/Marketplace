using System.Security.Claims;
using Marketplace.Application.DTOs.Common;
using Marketplace.Application.DTOs.Products;
using Marketplace.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Web.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService) => _productService = productService;

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts([FromQuery] ProductSearchRequest request)
    {
        var result = await _productService.GetProductsAsync(request);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        return Ok(product);
    }

    [HttpPost]
    [Authorize(Roles = "Seller,Admin")]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductRequest request)
    {
        var sellerId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var product = await _productService.CreateProductAsync(request, sellerId);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Seller,Admin")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(Guid id, UpdateProductRequest request)
    {
        var product = await _productService.UpdateProductAsync(id, request);
        return Ok(product);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Seller,Admin")]
    public async Task<ActionResult> DeleteProduct(Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
        var userId = Guid.Parse(userIdClaim);
        var isAdmin = User.IsInRole("Admin");
        await _productService.DeleteProductAsync(id, userId, isAdmin);
        return NoContent();
    }

    [HttpPatch("{id:guid}/stock")]
    [Authorize(Roles = "Seller,Admin")]
    public async Task<ActionResult> AdjustStock(Guid id, [FromBody] AdjustStockRequest request)
    {
        await _productService.AdjustStockAsync(id, request.Delta);
        return Ok();
    }
}
