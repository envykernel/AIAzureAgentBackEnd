namespace Domain.Services;

/// <summary>
/// Interface for token tracking service
/// </summary>
public interface ITokenTrackingService
{
    /// <summary>
    /// Records token usage for a specific session/thread
    /// </summary>
    /// <param name="sessionId">Unique identifier for the session (typically AgentThreadId)</param>
    /// <param name="tokenCount">Number of tokens used in this request</param>
    /// <returns>Token session information with accumulated data</returns>
    TokenSession RecordTokenUsage(string sessionId, int tokenCount);

    /// <summary>
    /// Gets current token session information without recording new usage
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    /// <returns>Token session information or null if session doesn't exist</returns>
    TokenSession? GetTokenSession(string sessionId);

    /// <summary>
    /// Resets token usage for a specific session
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    void ResetSession(string sessionId);

    /// <summary>
    /// Clears all token sessions (useful for cleanup)
    /// </summary>
    void ClearAllSessions();

    /// <summary>
    /// Gets the total number of active sessions
    /// </summary>
    int ActiveSessionCount { get; }
}
