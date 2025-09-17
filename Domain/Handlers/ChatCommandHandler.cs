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
            TokenUsage = new TokenUsageInfo
            {
                TokenCount = agentResponse.TokenUsage.TokenCount,
                TotalTokenCount = agentResponse.TokenUsage.TotalTokenCount,
                MaxTokens = _azureConfig.TokenLimits.AdvertisedMaxTokensPerSession, // Use advertised max tokens for display
                RemainingTokens = agentResponse.TokenUsage.RemainingTokens,
                TokenUsagePercentage = agentResponse.TokenUsage.TokenUsagePercentage
            },
            Session = new SessionInfo
            {
                AgentThreadId = agentResponse.Session.AgentThreadId,
                IsSessionClosed = agentResponse.Session.IsSessionClosed,
                SessionMessage = agentResponse.Session.SessionMessage
            },
            // Error handling properties
            IsError = agentResponse.IsError,
            ErrorType = agentResponse.ErrorType,
            ErrorMessage = agentResponse.ErrorMessage
        };
    }

    public async Task<bool> DeleteThreadAsync(string agentThreadId)
    {
        return await _chatService.DeleteThreadAsync(agentThreadId);
    }
}
