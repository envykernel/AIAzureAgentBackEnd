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
    /// Maximum tokens advertised to users per session/thread (default: 1000)
    /// This is what users see as their limit
    /// </summary>
    public int AdvertisedMaxTokensPerSession { get; set; } = 1000;
    
    /// <summary>
    /// Real maximum tokens allowed per session/thread (default: 1500)
    /// This is the actual limit used for blocking sessions
    /// </summary>
    public int RealMaxTokensPerSession { get; set; } = 1500;
    
    /// <summary>
    /// Warning threshold percentage when approaching advertised token limit (default: 90%)
    /// </summary>
    public double WarningThresholdPercentage { get; set; } = 90.0;
    
    /// <summary>
    /// Whether to automatically reset sessions when token limit is exceeded (default: false)
    /// </summary>
    public bool AutoResetOnLimitExceeded { get; set; } = false;
    
    /// <summary>
    /// Legacy property for backward compatibility - maps to AdvertisedMaxTokensPerSession
    /// </summary>
    public int MaxTokensPerSession 
    { 
        get => AdvertisedMaxTokensPerSession; 
        set => AdvertisedMaxTokensPerSession = value; 
    }
}
