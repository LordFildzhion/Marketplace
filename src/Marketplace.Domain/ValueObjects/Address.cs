using Marketplace.Domain.Common;

namespace Marketplace.Domain.ValueObjects;

public class Address : ValueObject
{
    public string Country { get; }
    public string City { get; }
    public string Street { get; }
    public string Building { get; }
    public string? Apartment { get; }
    public string ZipCode { get; }
    public string? AdditionalInfo { get; }

    private Address() { } // for EF Core

    public Address(string country, string city, string street, string building, string zipCode,
        string? apartment = null, string? additionalInfo = null)
    {
        if (string.IsNullOrWhiteSpace(country)) throw new ArgumentException("Country is required");
        if (string.IsNullOrWhiteSpace(city)) throw new ArgumentException("City is required");
        if (string.IsNullOrWhiteSpace(street)) throw new ArgumentException("Street is required");
        if (string.IsNullOrWhiteSpace(zipCode)) throw new ArgumentException("Zip code is required");

        Country = country;
        City = city;
        Street = street;
        Building = building;
        Apartment = apartment;
        ZipCode = zipCode;
        AdditionalInfo = additionalInfo;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Country;
        yield return City;
        yield return Street;
        yield return Building;
        yield return Apartment ?? string.Empty;
        yield return ZipCode;
        yield return AdditionalInfo ?? string.Empty;
    }

    public override string ToString() =>
        $"{Country}, {City}, {Street}, {Building}" +
        (Apartment != null ? $", apt. {Apartment}" : "") +
        $", {ZipCode}" +
        (AdditionalInfo != null ? $" ({AdditionalInfo})" : "");
}
