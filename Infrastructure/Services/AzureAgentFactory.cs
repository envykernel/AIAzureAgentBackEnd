using Domain.Interfaces;
using Domain.Configuration;
using Domain.DTOs;
using Microsoft.Extensions.Options;
using Azure.AI.Agents.Persistent;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Azure.Identity;

namespace Infrastructure.Services;

public class AzureAgentFactory : IAzureAgentFactory
{
    private readonly AzureConfiguration _azureConfig;

    public AzureAgentFactory(IOptions<AzureConfiguration> azureConfig)
    {
        _azureConfig = azureConfig.Value;
    }

    public async Task<AgentWithClient> GetAgentById(string id)
    {
        
        var credential = new DefaultAzureCredential();
        PersistentAgentsClient client = AzureAIAgent.CreateAgentsClient(_azureConfig.AgentEndpoint, credential);
        PersistentAgent definition = await client.Administration.GetAgentAsync(id);
        AzureAIAgent agent = new(definition, client);
       
        return new AgentWithClient(agent, client);
    }
}