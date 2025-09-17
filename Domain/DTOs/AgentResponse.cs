namespace Domain.DTOs;

public class AgentResponse
{
    public string Content { get; set; } = string.Empty;
    
    // Grouped data structures
    public TokenUsageInfo TokenUsage { get; set; } = new TokenUsageInfo();
    public SessionInfo Session { get; set; } = new SessionInfo();
    
    // Error handling properties
    public bool IsError { get; set; } = false;
    public string ErrorType { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}
