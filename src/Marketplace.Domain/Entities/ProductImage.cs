using Marketplace.Domain.Common;

namespace Marketplace.Domain.Entities;

public class ProductImage : BaseEntity
{
    public Guid ProductId { get; set; }      // <-- теперь set публичный
    public Product Product { get; set; } = null!;
    public string Url { get; private set; }
    public bool IsMain { get; private set; }
    public int SortOrder { get; private set; }

    public ProductImage(string url, bool isMain = false, int sortOrder = 0)
    {
        Url = url ?? throw new ArgumentNullException(nameof(url));
        IsMain = isMain;
        SortOrder = sortOrder;
    }
}
