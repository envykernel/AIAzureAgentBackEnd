using Domain.Interfaces;
using Domain.Services;
using Domain.Handlers;
using Domain.Configuration;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuration
        services.Configure<AzureConfiguration>(configuration.GetSection("Azure"));
        
        // Kernel Factory
        services.AddSingleton<IKernelFactory, KernelFactory>();
        
        // Repositories
        // No repositories needed - using Azure Agent Threads for conversation management
        
        // Azure Services
        services.AddScoped<IAzureAgentFactory, AzureAgentFactory>();
        
        // Domain Services
        services.AddScoped<IConfigurationValidationService, ConfigurationValidationService>();
        
        // Token Session Service (Singleton to maintain state across requests)
        services.AddSingleton<ITokenSessionService>(provider =>
        {
            var azureConfig = provider.GetRequiredService<IOptions<AzureConfiguration>>().Value;
            return new TokenSessionService(
                azureConfig.TokenLimits.MaxTokensPerSession, 
                azureConfig.TokenLimits.AutoResetOnLimitExceeded);
        });
        
        services.AddScoped<IChatService>(provider =>
        {
            var agentFactory = provider.GetRequiredService<IAzureAgentFactory>();
            var azureConfig = provider.GetRequiredService<IOptions<AzureConfiguration>>().Value;
            var kernelFactory = provider.GetRequiredService<IKernelFactory>();
            var configValidationService = provider.GetRequiredService<IConfigurationValidationService>();
            var tokenSessionService = provider.GetRequiredService<ITokenSessionService>();
            
            return new ChatService(agentFactory, azureConfig, kernelFactory, configValidationService, tokenSessionService);
        });
        
        // Command Handlers
        services.AddScoped<IChatCommandHandler, ChatCommandHandler>();
        
        return services;
    }
}
