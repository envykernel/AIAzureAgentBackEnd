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
        ChatSession? session;
        bool isNewSession = false;
        
        // Check if user has an active session
        if (string.IsNullOrEmpty(request.SessionId))
        {
            // Create new session
            session = await _chatService.CreateSessionAsync();
            isNewSession = true;
        }
        else
        {
            session = await _chatService.GetActiveSessionAsync(request.SessionId);
            if (session == null)
            {
                throw new InvalidOperationException("Invalid or inactive session ID");
            }
        }
        
        // Store user message
        var (userMessage, sessionAfterUserMessage) = await _chatService.SendMessageAsync(session.Id, request.Message, MessageRole.User, 0);
        
        // Get conversation context and generate agent response
        var conversation = await _chatService.GetConversationAsync(session.Id);

        //Keeponly the last message but always in a list
        conversation = new[] { conversation.Last() };

        var agentThreadId = string.IsNullOrEmpty(request.AgentThreadId) ? _azureConfig.DefaultAgentThreadId : request.AgentThreadId;
        var agentResponse = await _chatService.GenerateAgentResponseAsync(conversation, agentThreadId);

        var (agentMessage, finalSession) = await _chatService.SendMessageAsync(session.Id, agentResponse.Content, MessageRole.Assistant, agentResponse.TokenCount);
        
        return new ChatResponse
        {
            SessionId = finalSession.Id,
            Message = agentResponse.Content,
            Role = MessageRole.Assistant.ToString(),
            Timestamp = agentMessage.CreatedAt,
            TokenCount = agentResponse.TokenCount,
            IsNewSession = isNewSession,
            AgentThreadId = agentResponse.AgentThreadId,
            // Nouvelles propriétés pour la gestion des tokens et messages
            TotalMessageCount = finalSession.MessageCount,
            TotalTokenCount = finalSession.TokenCount,
            MaxTokens = finalSession.MaxTokens,
            RemainingTokens = finalSession.MaxTokens - finalSession.TokenCount,
            TokenUsagePercentage = (double)finalSession.TokenCount / finalSession.MaxTokens * 100.0
        };
    }
}
