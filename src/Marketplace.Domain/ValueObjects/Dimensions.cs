
namespace Marketplace.Domain.ValueObjects;
using Marketplace.Domain.Common;

public class Dimensions : ValueObject
{
    public decimal Length { get; }
    public decimal Width { get; }
    public decimal Height { get; }
    public decimal? Weight { get; }

    public Dimensions(decimal length, decimal width, decimal height, decimal? weight = null)
    {
        if (length <= 0) throw new ArgumentException("Length must be positive");
        if (width <= 0) throw new ArgumentException("Width must be positive");
        if (height <= 0) throw new ArgumentException("Height must be positive");
        if (weight.HasValue && weight.Value <= 0)
            throw new ArgumentException("Weight must be positive");

        Length = length;
        Width = width;
        Height = height;
        Weight = weight;
    }

    public decimal Volume => Length * Width * Height;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Length;
        yield return Width;
        yield return Height;
        yield return Weight ?? 0;
    }

    public override string ToString() =>
        $"{Length}x{Width}x{Height} cm" + (Weight.HasValue ? $", {Weight.Value} kg" : "");
}
