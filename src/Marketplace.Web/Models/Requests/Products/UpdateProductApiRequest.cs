
namespace Marketplace.Web.Models.Requests.Products;

public class UpdateProductApiRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? Stock { get; set; }
}
