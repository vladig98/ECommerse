namespace ECommerce.Identity.Settings;

public class OpenIDConfigSettings
{
    public required string Authority { get; set; }

    public required string ClientId { get; set; }

    public required string ClientSecret { get; set; }
}
