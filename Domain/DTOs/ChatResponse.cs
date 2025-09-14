namespace Domain.DTOs;

public class ChatResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int TokenCount { get; set; } = 0;
    public bool IsNewSession { get; set; } = false;
    public string AgentThreadId { get; set; } = string.Empty;
    
    // Nouvelles propriétés pour la gestion des tokens et messages
    public int TotalMessageCount { get; set; } = 0;
    public int TotalTokenCount { get; set; } = 0;
    public int MaxTokens { get; set; } = 4000;
    public int RemainingTokens { get; set; } = 4000;
    public double TokenUsagePercentage { get; set; } = 0.0;
}
