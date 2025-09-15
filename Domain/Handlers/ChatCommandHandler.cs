using Domain.DTOs;
using Domain.Interfaces;
using Domain.Services;
using Domain.Configuration;
using Microsoft.Extensions.Options;

namespace Domain.Handlers;

public class ChatCommandHandler : IChatCommandHandler
{
    private readonly IChatService _chatService;
    private readonly AzureConfiguration _azureConfig;

    public ChatCommandHandler(IChatService chatService, IOptions<AzureConfiguration> azureConfig)
    {
        _chatService = chatService;
        _azureConfig = azureConfig.Value;
    }

    public async Task<ChatResponse> HandleAsync(ChatRequest request)
    {
        var agentResponse = await _chatService.GenerateAgentResponseAsync(request.Message, request.AgentThreadId);
      
     
        return new ChatResponse
        {
            Message = agentResponse.Content,
            Role = "Assistant",
            Timestamp = DateTime.UtcNow,
            TokenCount = agentResponse.TokenCount,
            AgentThreadId = agentResponse.AgentThreadId,
            // Use actual token usage values from agent response
            TotalTokenCount = agentResponse.TotalTokenCount,
            MaxTokens = _azureConfig.TokenLimits.MaxTokensPerSession, // Use configured max tokens
            RemainingTokens = agentResponse.RemainingTokens,
            TokenUsagePercentage = agentResponse.TokenUsagePercentage
        };
    }
}
