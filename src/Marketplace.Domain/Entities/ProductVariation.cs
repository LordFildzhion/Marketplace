
namespace Marketplace.Domain.Entities;
using Marketplace.Domain.Common;
using Marketplace.Domain.ValueObjects;

public class ProductVariation : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; }
    public string VariationType { get; private set; }
    public string VariationValue { get; private set; }
    public Money? AdditionalPrice { get; private set; }
    public int Stock { get; private set; }
    private ProductVariation() { } // for EF Core
}
