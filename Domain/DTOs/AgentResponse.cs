namespace Domain.DTOs;

public class AgentResponse
{
    public string Content { get; set; } = string.Empty;
    public int TokenCount { get; set; } = 0;
    public string AgentThreadId { get; set; } = string.Empty;
}
