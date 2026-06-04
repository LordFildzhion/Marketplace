
namespace Marketplace.Domain.ValueObjects;
using Marketplace.Domain.Common;

public class Email : ValueObject
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty", nameof(value));
        if (!IsValidEmail(value))
            throw new ArgumentException($"Invalid email format: {value}", nameof(value));
        Value = value.ToLowerInvariant();
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
    public static implicit operator Email(string value) => new(value);
    public static explicit operator string(Email email) => email.Value;
}
