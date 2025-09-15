using System.Collections.Concurrent;
using Domain.Exceptions;

namespace Domain.Services;

public interface ITokenSessionService
{
    TokenSessionInfo GetOrCreateSession(string agentThreadId);
    void UpdateSession(string agentThreadId, int additionalTokens);
    void ResetSession(string agentThreadId);
    bool CheckTokenLimit(string agentThreadId, int additionalTokens, bool autoResetOnExceeded = false);
}

public class TokenSessionService : ITokenSessionService
{
    private readonly ConcurrentDictionary<string, TokenSessionInfo> _sessions = new();
    private readonly int _maxTokensPerSession;
    private readonly bool _autoResetOnLimitExceeded;

    public TokenSessionService(int maxTokensPerSession, bool autoResetOnLimitExceeded = false)
    {
        _maxTokensPerSession = maxTokensPerSession;
        _autoResetOnLimitExceeded = autoResetOnLimitExceeded;
    }

    public TokenSessionInfo GetOrCreateSession(string agentThreadId)
    {
        return _sessions.GetOrAdd(agentThreadId, _ => new TokenSessionInfo
        {
            AgentThreadId = agentThreadId,
            TotalTokenCount = 0,
            MaxTokens = _maxTokensPerSession,
            RemainingTokens = _maxTokensPerSession,
            TokenUsagePercentage = 0.0
        });
    }

    public void UpdateSession(string agentThreadId, int additionalTokens)
    {
        var session = GetOrCreateSession(agentThreadId);
        session.TotalTokenCount += additionalTokens;
        session.RemainingTokens = Math.Max(0, _maxTokensPerSession - session.TotalTokenCount);
        session.TokenUsagePercentage = _maxTokensPerSession > 0 ? (double)session.TotalTokenCount / _maxTokensPerSession * 100.0 : 0.0;
    }

    public bool CheckTokenLimit(string agentThreadId, int additionalTokens, bool autoResetOnExceeded = false)
    {
        var session = GetOrCreateSession(agentThreadId);
        var projectedTotal = session.TotalTokenCount + additionalTokens;
        
        if (projectedTotal > _maxTokensPerSession)
        {
            if (autoResetOnExceeded || _autoResetOnLimitExceeded)
            {
                ResetSession(agentThreadId);
                return true; // Allow the request after reset
            }
            else
            {
                throw new TokenLimitExceededException(agentThreadId, session.TotalTokenCount, _maxTokensPerSession, false);
            }
        }
        
        return true; // Within limits
    }

    public void ResetSession(string agentThreadId)
    {
        if (_sessions.TryGetValue(agentThreadId, out var session))
        {
            session.TotalTokenCount = 0;
            session.RemainingTokens = _maxTokensPerSession;
            session.TokenUsagePercentage = 0.0;
        }
    }
}

public class TokenSessionInfo
{
    public string AgentThreadId { get; set; } = string.Empty;
    public int TotalTokenCount { get; set; } = 0;
    public int MaxTokens { get; set; } = 0;
    public int RemainingTokens { get; set; } = 0;
    public double TokenUsagePercentage { get; set; } = 0.0;
}
