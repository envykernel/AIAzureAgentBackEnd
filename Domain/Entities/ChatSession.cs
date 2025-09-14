namespace Domain.Entities;

public class ChatSession
{
    public string Id { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public int MessageCount { get; set; } = 0;
    public int TokenCount { get; set; } = 0;
    public int MaxTokens { get; set; } = 4000; // Default token limit
}
