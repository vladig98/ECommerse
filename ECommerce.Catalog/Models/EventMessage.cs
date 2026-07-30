namespace ECommerce.Catalog.Models;

public class EventMessage : BaseModel
{
    public string Key { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
