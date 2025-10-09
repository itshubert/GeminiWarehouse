namespace GeminiWarehouse.Application.Common.Models.Jobs;

public sealed record JobModel(
    Guid Id,
    string ExternalId,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    ShippingAddressModel ShippingAddress,
    List<JobItemModel> Items);

public sealed record ShippingAddressModel(
    string FirstName,
    string LastName,
    string AddressLine1,
    string AddressLine2,
    string City,
    string State,
    string PostCode,
    string Country);

public sealed record JobItemModel(
    Guid? Id,
    Guid JobId,
    Guid ProductId,
    string ProductName,
    int Quantity);