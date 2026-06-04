
namespace Marketplace.Web.Models.Requests.Cart;

public class AddToCartApiRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}
