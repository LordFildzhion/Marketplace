
namespace Marketplace.Domain.ValueObjects;
using Marketplace.Domain.Common;

public class Rating : ValueObject
{
    public int Value { get; }

    public Rating(int value)
    {
        if (value < 1 || value > 5)
            throw new ArgumentException("Rating must be between 1 and 5", nameof(value));
        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => $"{Value}/5";
    public static bool operator >(Rating left, Rating right) => left.Value > right.Value;
    public static bool operator <(Rating left, Rating right) => left.Value < right.Value;
}
