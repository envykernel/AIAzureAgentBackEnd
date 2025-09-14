using Domain.DTOs;
using Domain.Entities;
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
        Message message = new Message
        {
            Content = request.Message,
            Role = MessageRole.User,
            CreatedAt = DateTime.UtcNow
        };
        var agentResponse = await _chatService.GenerateAgentResponseAsync(message, request.AgentThreadId);
      
     
        return new ChatResponse
        {
            SessionId = agentResponse.AgentThreadId,
            Message = agentResponse.Content,
            Role = MessageRole.Assistant.ToString(),
            Timestamp = DateTime.UtcNow,
            TokenCount = agentResponse.TokenCount,
            IsNewSession = string.IsNullOrEmpty(request.AgentThreadId),
            AgentThreadId = agentResponse.AgentThreadId,
            // Nouvelles propriétés pour la gestion des tokens et messages
            TotalMessageCount = 0,
            TotalTokenCount = 0,
            MaxTokens = 0,
            RemainingTokens = 0,//finalSession.MaxTokens - finalSession.TokenCount,
            TokenUsagePercentage = 0//(double)finalSession.TokenCount / finalSession.MaxTokens * 100.0
        };
    }
}
