namespace Domain.DTOs;

public class ChatResponse
{
    public string Message { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string AgentThreadId { get; set; } = string.Empty;
    
    // Token usage properties
    public int TokenCount { get; set; } = 0;
    public int TotalTokenCount { get; set; } = 0;
    public int MaxTokens { get; set; } = 0;
    public int RemainingTokens { get; set; } = 0;
    public double TokenUsagePercentage { get; set; } = 0.0;
}
