namespace ECommerse.Identity.Settings;

public sealed class JwtConfigSettings
{
    public required string AuthenticationScheme { get; set; }

    public required string MetadataAddress { get; set; }

    public required string Authority { get; set; }

    public required string Audience { get; set; }

    public string[] ValidAudiences { get; set; } = [];

    public string[] ValidIssuers { get; set; } = [];
}
