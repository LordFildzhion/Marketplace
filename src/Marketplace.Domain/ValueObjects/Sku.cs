
namespace Marketplace.Domain.ValueObjects;
using Marketplace.Domain.Common;

public class Sku : ValueObject
{
    public string Value { get; }

    public Sku(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("SKU cannot be empty", nameof(value));
        if (value.Length < 3 || value.Length > 50)
            throw new ArgumentException("SKU must be between 3 and 50 characters", nameof(value));
        if (!System.Text.RegularExpressions.Regex.IsMatch(value, @"^[A-Z0-9\-]+$"))
            throw new ArgumentException("SKU must contain only uppercase letters, numbers, and hyphens", nameof(value));

        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
