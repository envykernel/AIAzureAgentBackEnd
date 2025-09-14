
using Microsoft.SemanticKernel.Agents.AzureAI;
using Domain.DTOs;

namespace Domain.Interfaces;

public interface IAzureAgentFactory
{
    Task<AgentWithClient> GetAgentById(string id);
}
