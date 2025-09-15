namespace Domain.Configuration;

public class AzureConfiguration
{
    public string AgentEndpoint { get; set; } = string.Empty;
    public string SAVAgentId { get; set; } = string.Empty;
    public string DefaultAgentThreadId { get; set; } = string.Empty;
    public AzureOpenAIConfiguration OpenAI { get; set; } = new();
    public TokenConfiguration TokenLimits { get; set; } = new();
}

public class AzureOpenAIConfiguration
{
    public string Endpoint { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}

public class TokenConfiguration
{
    /// <summary>
    /// Maximum tokens allowed per session/thread (default: 100,000)
    /// </summary>
    public int MaxTokensPerSession { get; set; } = 100000;
    
    /// <summary>
    /// Warning threshold percentage when approaching token limit (default: 90%)
    /// </summary>
    public double WarningThresholdPercentage { get; set; } = 90.0;
    
    /// <summary>
    /// Whether to automatically reset sessions when token limit is exceeded (default: false)
    /// </summary>
    public bool AutoResetOnLimitExceeded { get; set; } = false;
}
