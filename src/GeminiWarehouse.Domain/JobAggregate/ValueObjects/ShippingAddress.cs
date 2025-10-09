using GeminiWarehouse.Domain.Common.Models;

namespace GeminiWarehouse.Domain.JobAggregate.ValueObjects;

public sealed class ShippingAddress : ValueObject
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string AddressLine1 { get; private set; }
    public string AddressLine2 { get; private set; } = string.Empty;
    public string City { get; private set; }
    public string State { get; private set; }
    public string PostCode { get; private set; }
    public string Country { get; private set; }

    private ShippingAddress(
        string firstName,
        string lastName,
        string addressLine1,
        string addressLine2,
        string city,
        string state,
        string postCode,
        string country)
    {
        FirstName = firstName;
        LastName = lastName;
        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2;
        City = city;
        State = state;
        PostCode = postCode;
        Country = country;
    }

    public static ShippingAddress Create(
        string firstName,
        string lastName,
        string addressLine1,
        string? addressLine2,
        string city,
        string state,
        string postCode,
        string country)
    {
        return new ShippingAddress(
            firstName,
            lastName,
            addressLine1,
            addressLine2 ?? string.Empty,
            city,
            state,
            postCode,
            country);
    }



    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return AddressLine1;
        yield return AddressLine2;
        yield return City;
        yield return State;
        yield return PostCode;
        yield return Country;
    }
}