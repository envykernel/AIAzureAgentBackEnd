using System.Collections.Concurrent;

namespace Domain.Services;

/// <summary>
/// Service for tracking token usage across different agent threads/sessions
/// Uses in-memory storage without database persistence
/// </summary>
public class TokenTrackingService : ITokenTrackingService
{
    private readonly ConcurrentDictionary<string, TokenSession> _tokenSessions = new();
    private readonly int _maxTokensPerSession;

    public TokenTrackingService(int maxTokensPerSession = 100000) // Default 100k tokens
    {
        _maxTokensPerSession = maxTokensPerSession;
    }

    /// <summary>
    /// Records token usage for a specific session/thread
    /// </summary>
    /// <param name="sessionId">Unique identifier for the session (typically AgentThreadId)</param>
    /// <param name="tokenCount">Number of tokens used in this request</param>
    /// <returns>Token session information with accumulated data</returns>
    public TokenSession RecordTokenUsage(string sessionId, int tokenCount)
    {
        var session = _tokenSessions.AddOrUpdate(
            sessionId,
            new TokenSession(sessionId, tokenCount, _maxTokensPerSession),
            (key, existing) => existing.AddTokens(tokenCount)
        );

        return session;
    }

    /// <summary>
    /// Gets current token session information without recording new usage
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    /// <returns>Token session information or null if session doesn't exist</returns>
    public TokenSession? GetTokenSession(string sessionId)
    {
        return _tokenSessions.TryGetValue(sessionId, out var session) ? session : null;
    }

    /// <summary>
    /// Resets token usage for a specific session
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    public void ResetSession(string sessionId)
    {
        _tokenSessions.TryRemove(sessionId, out _);
    }

    /// <summary>
    /// Clears all token sessions (useful for cleanup)
    /// </summary>
    public void ClearAllSessions()
    {
        _tokenSessions.Clear();
    }

    /// <summary>
    /// Gets the total number of active sessions
    /// </summary>
    public int ActiveSessionCount => _tokenSessions.Count;
}

/// <summary>
/// Represents token usage information for a single session
/// </summary>
public class TokenSession
{
    public string SessionId { get; }
    public int TotalTokenCount { get; private set; }
    public int MaxTokensPerSession { get; }
    public int RemainingTokens { get; private set; }
    public double TokenUsagePercentage { get; private set; }
    public DateTime LastUpdated { get; private set; }

    public TokenSession(string sessionId, int initialTokenCount, int maxTokensPerSession)
    {
        SessionId = sessionId;
        MaxTokensPerSession = maxTokensPerSession;
        TotalTokenCount = initialTokenCount;
        LastUpdated = DateTime.UtcNow;
        UpdateCalculatedFields();
    }

    /// <summary>
    /// Adds tokens to this session and returns a new instance
    /// </summary>
    /// <param name="additionalTokens">Number of tokens to add</param>
    /// <returns>New TokenSession instance with updated counts</returns>
    public TokenSession AddTokens(int additionalTokens)
    {
        var newSession = new TokenSession(SessionId, TotalTokenCount + additionalTokens, MaxTokensPerSession);
        return newSession;
    }

    private void UpdateCalculatedFields()
    {
        RemainingTokens = Math.Max(0, MaxTokensPerSession - TotalTokenCount);
        TokenUsagePercentage = MaxTokensPerSession > 0 
            ? Math.Round((double)TotalTokenCount / MaxTokensPerSession * 100, 2) 
            : 0;
    }

    /// <summary>
    /// Checks if the session has exceeded the token limit
    /// </summary>
    public bool IsTokenLimitExceeded => TotalTokenCount >= MaxTokensPerSession;

    /// <summary>
    /// Checks if the session is approaching the token limit (90% threshold)
    /// </summary>
    public bool IsApproachingTokenLimit => TokenUsagePercentage >= 90.0;
}
