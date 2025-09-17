namespace Domain.Exceptions;

public class ConfigurationException : Exception
{
    public ConfigurationException(string message) : base(message)
    {
    }

    public ConfigurationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class AzureConfigurationException : ConfigurationException
{
    public AzureConfigurationException(string message) : base(message)
    {
    }

    public AzureConfigurationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class TokenLimitExceededException : Exception
{
    public string AgentThreadId { get; }
    public int CurrentTokenCount { get; }
    public int MaxTokens { get; }
    public bool ShouldResetSession { get; }

    public TokenLimitExceededException(string agentThreadId, int currentTokenCount, int maxTokens, bool shouldResetSession = false)
        : base($"Token limit exceeded for session '{agentThreadId}'. Current: {currentTokenCount}, Max: {maxTokens}. Please create a new session.")
    {
        AgentThreadId = agentThreadId;
        CurrentTokenCount = currentTokenCount;
        MaxTokens = maxTokens;
        ShouldResetSession = shouldResetSession;
    }

    public TokenLimitExceededException(string agentThreadId, int currentTokenCount, int maxTokens, bool shouldResetSession, string customMessage)
        : base(customMessage)
    {
        AgentThreadId = agentThreadId;
        CurrentTokenCount = currentTokenCount;
        MaxTokens = maxTokens;
        ShouldResetSession = shouldResetSession;
    }
}
