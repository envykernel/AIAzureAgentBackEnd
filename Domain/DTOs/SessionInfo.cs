namespace Domain.DTOs;

public class SessionInfo
{
    public string AgentThreadId { get; set; } = string.Empty;
    public bool IsSessionClosed { get; set; } = false;
    public string SessionMessage { get; set; } = string.Empty;
}
