using System.Collections.Concurrent;
using Domain.Exceptions;

namespace Domain.Services;

public interface ITokenSessionService
{
    TokenSessionInfo GetOrCreateSession(string agentThreadId);
    void UpdateSession(string agentThreadId, int additionalTokens);
    void ResetSession(string agentThreadId);
    bool CheckTokenLimit(string agentThreadId, int additionalTokens, bool autoResetOnExceeded = false);
    void BlockSession(string agentThreadId);
}

public class TokenSessionService : ITokenSessionService
{
    private readonly ConcurrentDictionary<string, TokenSessionInfo> _sessions = new();
    private readonly int _advertisedMaxTokensPerSession;
    private readonly int _realMaxTokensPerSession;
    private readonly bool _autoResetOnLimitExceeded;
    private readonly double _warningThresholdPercentage;

    public TokenSessionService(int advertisedMaxTokensPerSession, int realMaxTokensPerSession, bool autoResetOnLimitExceeded = false, double warningThresholdPercentage = 90.0)
    {
        _advertisedMaxTokensPerSession = advertisedMaxTokensPerSession;
        _realMaxTokensPerSession = realMaxTokensPerSession;
        _autoResetOnLimitExceeded = autoResetOnLimitExceeded;
        _warningThresholdPercentage = warningThresholdPercentage;
    }

    public TokenSessionInfo GetOrCreateSession(string agentThreadId)
    {
        return _sessions.GetOrAdd(agentThreadId, _ => new TokenSessionInfo
        {
            AgentThreadId = agentThreadId,
            TotalTokenCount = 0,
            MaxTokens = _advertisedMaxTokensPerSession, // Show advertised limit to users
            RemainingTokens = _advertisedMaxTokensPerSession,
            TokenUsagePercentage = 0.0,
            IsBlocked = false,
            IsWarningThresholdReached = false
        });
    }

    public void UpdateSession(string agentThreadId, int additionalTokens)
    {
        var session = GetOrCreateSession(agentThreadId);
        session.TotalTokenCount += additionalTokens;
        
        // Calculate remaining tokens and percentage based on ADVERTISED limit (what users see)
        session.RemainingTokens = Math.Max(0, _advertisedMaxTokensPerSession - session.TotalTokenCount);
        session.TokenUsagePercentage = _advertisedMaxTokensPerSession > 0 ? (double)session.TotalTokenCount / _advertisedMaxTokensPerSession * 100.0 : 0.0;
        
        // Check if warning threshold is reached (based on advertised limit)
        session.IsWarningThresholdReached = session.TokenUsagePercentage >= _warningThresholdPercentage;
        
        // Note: Session blocking is now handled in CheckTokenLimit method
    }

    public bool CheckTokenLimit(string agentThreadId, int additionalTokens, bool autoResetOnExceeded = false)
    {
        var session = GetOrCreateSession(agentThreadId);
        
        // If session is already blocked, prevent new requests
        if (session.IsBlocked)
        {
            throw new TokenLimitExceededException(agentThreadId, session.TotalTokenCount, _advertisedMaxTokensPerSession, false, "Session is blocked due to previous token limit exceeded");
        }
        
        var projectedTotal = session.TotalTokenCount + additionalTokens;
        
        // Block session immediately if this request would exceed the REAL limit
        if (projectedTotal > _realMaxTokensPerSession)
        {
            // Mark session as blocked immediately
            session.IsBlocked = true;
            throw new TokenLimitExceededException(agentThreadId, session.TotalTokenCount, _advertisedMaxTokensPerSession, false, 
                $"Session blocked: This request would exceed the real token limit. Current: {session.TotalTokenCount}, Request: {additionalTokens}, Real Limit: {_realMaxTokensPerSession}");
        }
        
        // Allow requests that exceed advertised limit but not real limit
        return true;
    }

    public void ResetSession(string agentThreadId)
    {
        if (_sessions.TryGetValue(agentThreadId, out var session))
        {
            session.TotalTokenCount = 0;
            session.RemainingTokens = _advertisedMaxTokensPerSession;
            session.TokenUsagePercentage = 0.0;
            session.IsBlocked = false;
            session.IsWarningThresholdReached = false;
        }
    }

    public void BlockSession(string agentThreadId)
    {
        var session = GetOrCreateSession(agentThreadId);
        session.IsBlocked = true;
    }
}

public class TokenSessionInfo
{
    public string AgentThreadId { get; set; } = string.Empty;
    public int TotalTokenCount { get; set; } = 0;
    public int MaxTokens { get; set; } = 0;
    public int RemainingTokens { get; set; } = 0;
    public double TokenUsagePercentage { get; set; } = 0.0;
    public bool IsBlocked { get; set; } = false;
    public bool IsWarningThresholdReached { get; set; } = false;
}
