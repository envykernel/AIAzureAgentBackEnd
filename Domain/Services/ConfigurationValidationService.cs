using Domain.Configuration;
using Domain.Exceptions;

namespace Domain.Services;

public interface IConfigurationValidationService
{
    void ValidateAzureConfiguration(AzureConfiguration configuration);
}

public class ConfigurationValidationService : IConfigurationValidationService
{
    public void ValidateAzureConfiguration(AzureConfiguration configuration)
    {
        if (configuration == null)
        {
            throw new AzureConfigurationException("Azure configuration is null");
        }

        var errors = new List<string>();

        // Validate AgentEndpoint
        if (string.IsNullOrWhiteSpace(configuration.AgentEndpoint))
        {
            errors.Add("AgentEndpoint is required");
        }
        else if (!Uri.TryCreate(configuration.AgentEndpoint, UriKind.Absolute, out _))
        {
            errors.Add("AgentEndpoint must be a valid URL");
        }

        // Validate SAVAgentId
        if (string.IsNullOrWhiteSpace(configuration.SAVAgentId))
        {
            errors.Add("SAVAgentId is required");
        }

        // Validate OpenAI configuration
        if (configuration.OpenAI == null)
        {
            errors.Add("OpenAI configuration is required");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(configuration.OpenAI.Endpoint))
            {
                errors.Add("OpenAI Endpoint is required");
            }
            else if (!Uri.TryCreate(configuration.OpenAI.Endpoint, UriKind.Absolute, out _))
            {
                errors.Add("OpenAI Endpoint must be a valid URL");
            }

            if (string.IsNullOrWhiteSpace(configuration.OpenAI.DeploymentName))
            {
                errors.Add("OpenAI DeploymentName is required");
            }

            if (string.IsNullOrWhiteSpace(configuration.OpenAI.ApiKey))
            {
                errors.Add("OpenAI ApiKey is required");
            }
        }

        // Validate Token configuration
        if (configuration.TokenLimits == null)
        {
            errors.Add("TokenLimits configuration is required");
        }
        else
        {
            if (configuration.TokenLimits.MaxTokensPerSession <= 0)
            {
                errors.Add("MaxTokensPerSession must be greater than 0");
            }

            if (configuration.TokenLimits.WarningThresholdPercentage < 0 || configuration.TokenLimits.WarningThresholdPercentage > 100)
            {
                errors.Add("WarningThresholdPercentage must be between 0 and 100");
            }
        }

        if (errors.Any())
        {
            var errorMessage = $"Azure configuration validation failed: {string.Join(", ", errors)}";
            throw new AzureConfigurationException(errorMessage);
        }
    }
}
