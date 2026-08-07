namespace ECommerce.Catalog.Setings;

public class KafkaSettings()
{
    public string Server { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string SchemaRegistryUrl { get; init; } = string.Empty;
}
