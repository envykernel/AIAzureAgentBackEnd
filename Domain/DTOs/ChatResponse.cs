namespace Domain.DTOs;

public class ChatResponse
{
    public string Message { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Grouped data structures
    public TokenUsageInfo TokenUsage { get; set; } = new TokenUsageInfo();
    public SessionInfo Session { get; set; } = new SessionInfo();
    
    // Error handling properties
    public bool IsError { get; set; } = false;
    public string ErrorType { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}
