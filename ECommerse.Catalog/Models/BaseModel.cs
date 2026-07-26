namespace ECommerse.Catalog.Models;

public abstract class BaseModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ConcurrencyCheck]
    public Guid Version { get; set; } = Guid.NewGuid();
}
