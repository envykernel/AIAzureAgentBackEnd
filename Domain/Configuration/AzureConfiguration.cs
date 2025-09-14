namespace Domain.Configuration;

public class AzureConfiguration
{
    public string AgentEndpoint { get; set; } = string.Empty;
    public string SAVAgentId { get; set; } = string.Empty;
    public string DefaultAgentThreadId { get; set; } = string.Empty;
    public AzureOpenAIConfiguration OpenAI { get; set; } = new();
}

public class AzureOpenAIConfiguration
{
    public string Endpoint { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}
