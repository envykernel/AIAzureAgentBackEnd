using Microsoft.SemanticKernel.Agents.AzureAI;
using Azure.AI.Agents.Persistent;

namespace Domain.DTOs;

public class AgentWithClient
{
    public AzureAIAgent Agent { get; set; }
    public PersistentAgentsClient Client { get; set; }

    public AgentWithClient(AzureAIAgent agent, PersistentAgentsClient client)
    {
        Agent = agent;
        Client = client;
    }
}
