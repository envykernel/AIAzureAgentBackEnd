namespace Domain.Entities;

public enum MessageRole
{
    User,
    Assistant,
    System
}

public class Message
{
    public string Id { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public MessageRole Role { get; set; }
    public string Content { get; set; } = string.Empty;
    public int TokenCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; set; }
}
