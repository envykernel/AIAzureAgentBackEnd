namespace Domain.Entities;

public class ConversationSummary
{
    public string Id { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public int TokenCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    public List<string> KeyPoints { get; set; } = new();
    public Dictionary<string, object>? Context { get; set; }
}
