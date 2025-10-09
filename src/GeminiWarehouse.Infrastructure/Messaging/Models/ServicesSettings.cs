namespace GeminiOrderFulfillment.Infrastructure.Messaging.Models;

public sealed class ServicesSettings
{
    public string CatalogServiceBaseUrl { get; set; } = string.Empty;
    public string CustomerServiceBaseUrl { get; set; } = string.Empty;
}