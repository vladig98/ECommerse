namespace ECommerce.Catalog.Models;

public class DeadLetterMessage : BaseModel
{
    public Guid? OriginalMessageId { get; set; }
    public string Source { get; set; } = string.Empty;
    public string? Key { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string ErrorReason { get; set; } = string.Empty;
    public DateTime FailedAt { get; set; } = DateTime.UtcNow;
}
