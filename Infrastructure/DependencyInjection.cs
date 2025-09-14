using Domain.Interfaces;
using Domain.Services;
using Domain.Handlers;
using Domain.Configuration;
using Infrastructure.MongoDb;
using Infrastructure.Repositories;
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
        
        // MongoDB Client
        services.AddSingleton<MongoDbClient>();
        
        // Kernel Factory
        services.AddSingleton<IKernelFactory, KernelFactory>();
        
        // Repositories
        services.AddScoped<IChatSessionRepository, ChatSessionRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IConversationSummaryRepository, ConversationSummaryRepository>();
        
        // Azure Services
        services.AddScoped<IAzureAgentFactory, AzureAgentFactory>();
        
        // Domain Services
        services.AddScoped<IChatService>(provider =>
        {
            var sessionRepo = provider.GetRequiredService<IChatSessionRepository>();
            var messageRepo = provider.GetRequiredService<IMessageRepository>();
            var summaryRepo = provider.GetRequiredService<IConversationSummaryRepository>();
            var agentFactory = provider.GetRequiredService<IAzureAgentFactory>();
            var azureConfig = provider.GetRequiredService<IOptions<AzureConfiguration>>().Value;
            var kernelFactory = provider.GetRequiredService<IKernelFactory>();
            
            return new ChatService(sessionRepo, messageRepo, summaryRepo, agentFactory, azureConfig, kernelFactory);
        });
        
        // Command Handlers
        services.AddScoped<IChatCommandHandler, ChatCommandHandler>();
        
        return services;
    }
}
