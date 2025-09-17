using Domain.Interfaces;
using Domain.Configuration;
using Domain.DTOs;
using Domain.Exceptions;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Azure.AI.Agents.Persistent;
using Microsoft.SemanticKernel.Agents;
using System.Data.Common;
using Domain.Services;


namespace Domain.Services;

public class ChatService : IChatService
{
    private const string USAGE_METADATA_KEY = "Usage";
    private const string TOTAL_TOKENS_PROPERTY = "TotalTokens";
    
    private readonly IAzureAgentFactory _azureAgentFactory;
    private readonly AzureAIAgent masterAgent;
    private readonly Kernel _kernel;
    private readonly IConfigurationValidationService _configurationValidationService;
    private readonly AzureConfiguration _azureConfiguration;
    private readonly ITokenSessionService _tokenSessionService;

     private const string TranslatorName = "NumeroTranslator";
    private const string TranslatorInstructions = "Add one to latest user number and spell it in spanish without explanation.";


    public ChatService(
        IAzureAgentFactory azureAgentFactory,
        AzureConfiguration azureConfiguration,
        IKernelFactory kernelFactory,
        IConfigurationValidationService configurationValidationService,
        ITokenSessionService tokenSessionService)
    {
        _azureAgentFactory = azureAgentFactory;
        _kernel = kernelFactory.CreateKernel();
        _configurationValidationService = configurationValidationService;
        _azureConfiguration = azureConfiguration;
        _tokenSessionService = tokenSessionService;
        
        // Validate configuration before proceeding
        _configurationValidationService.ValidateAzureConfiguration(azureConfiguration);
        
        // Initialize masterAgent with validated configuration
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"Getting agent by id: {azureConfiguration.SAVAgentId}");
        Console.ResetColor();
        
        try
        {
            var agentWithClient = _azureAgentFactory.GetAgentById(azureConfiguration.SAVAgentId).Result;
            masterAgent = agentWithClient.Agent;
        }
        catch (Exception ex)
        {
            throw new AzureConfigurationException($"Failed to initialize Azure Agent with ID '{azureConfiguration.SAVAgentId}': {ex.Message}", ex);
        }
    }

    public async Task<AgentResponse> GenerateAgentResponseAsync(string userMessageContent, string? agentThreadId = null)
    {
        var thread = CreateOrGetThread(agentThreadId);

        try
        {
            Console.WriteLine($"Generating agent response for message: {userMessageContent}");
            
            await foreach (ChatMessageContent response in masterAgent.InvokeAsync(new ChatMessageContent(AuthorRole.User, userMessageContent), thread))
            {
                // Get the thread ID from the Azure thread after it's been used
                var sessionId = thread.Id;
                if (string.IsNullOrEmpty(sessionId))
                {
                    throw new InvalidOperationException("Azure Agent Thread did not provide a valid thread ID after invocation");
                }
                
                var tokenCount = ExtractTotalTokenCount(response);
                
                // Check token limit after we know the actual token count
                try
                {
                    _tokenSessionService.CheckTokenLimit(sessionId, tokenCount, _azureConfiguration.TokenLimits.AutoResetOnLimitExceeded);
                }
                catch (TokenLimitExceededException)
                {
                    // This request would exceed the real limit, but we continue processing
                    // The real limit check is handled by TokenSessionService
                }
                
                // Update token session with new tokens
                _tokenSessionService.UpdateSession(sessionId, tokenCount);
                var tokenSession = _tokenSessionService.GetOrCreateSession(sessionId);
                
                Console.WriteLine($"Agent response received: {response.Content}");
                Console.WriteLine($"Token count: {tokenCount}, Total: {tokenSession.TotalTokenCount}, Remaining: {tokenSession.RemainingTokens}, Usage: {tokenSession.TokenUsagePercentage:F2}%");
                
                // Generate session closure message if advertised limit is exceeded
                string sessionMessage = string.Empty;
                bool isSessionClosed = false;
                
                // Check if we've exceeded the advertised limit (what users see)
                if (tokenSession.TotalTokenCount > _azureConfiguration.TokenLimits.AdvertisedMaxTokensPerSession)
                {
                    // This response exceeds the advertised limit, so we close the session gracefully
                    sessionMessage = $"Session closed: You have used {tokenSession.TokenUsagePercentage:F1}% of your token limit ({tokenSession.TotalTokenCount}/{tokenSession.MaxTokens}). Please create a new session for future requests.";
                    isSessionClosed = true;
                    // Block the session for future requests
                    _tokenSessionService.BlockSession(sessionId);
                }
                
                return new AgentResponse
                {
                    Content = response.Content ?? "No content in response.",
                    TokenUsage = new TokenUsageInfo
                    {
                        TokenCount = tokenCount,
                        TotalTokenCount = tokenSession.TotalTokenCount,
                        RemainingTokens = tokenSession.RemainingTokens,
                        TokenUsagePercentage = tokenSession.TokenUsagePercentage,
                        MaxTokens = tokenSession.MaxTokens
                    },
                    Session = new SessionInfo
                    {
                        AgentThreadId = sessionId,
                        IsSessionClosed = isSessionClosed,
                        SessionMessage = sessionMessage
                    }
                };
            }
            
            // Handle case where no response is generated
            var fallbackSessionId = thread.Id;
            if (string.IsNullOrEmpty(fallbackSessionId))
            {
                throw new InvalidOperationException("Azure Agent Thread did not provide a valid thread ID after invocation");
            }
            
            var fallbackTokenSession = _tokenSessionService.GetOrCreateSession(fallbackSessionId);
            
            return new AgentResponse
            {
                Content = "No response generated from agent.",
                TokenUsage = new TokenUsageInfo
                {
                    TokenCount = 0,
                    TotalTokenCount = fallbackTokenSession.TotalTokenCount,
                    RemainingTokens = fallbackTokenSession.RemainingTokens,
                    TokenUsagePercentage = fallbackTokenSession.TokenUsagePercentage,
                    MaxTokens = fallbackTokenSession.MaxTokens
                },
                Session = new SessionInfo
                {
                    AgentThreadId = fallbackSessionId,
                    IsSessionClosed = false,
                    SessionMessage = string.Empty
                }
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GenerateAgentResponseAsync: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            
            // Log the exception in production
            var errorSessionId = thread?.Id;
            if (string.IsNullOrEmpty(errorSessionId))
            {
                // If we can't get a thread ID, we can't track tokens
                return new AgentResponse
                {
                    Content = $"I encountered an error while processing your request: {ex.Message}",
                    TokenUsage = new TokenUsageInfo(),
                    Session = new SessionInfo()
                };
            }
            
            var errorTokenSession = _tokenSessionService.GetOrCreateSession(errorSessionId);
            
            return new AgentResponse
            {
                Content = $"I encountered an error while processing your request: {ex.Message}",
                TokenUsage = new TokenUsageInfo
                {
                    TokenCount = 0,
                    TotalTokenCount = errorTokenSession.TotalTokenCount,
                    RemainingTokens = errorTokenSession.RemainingTokens,
                    TokenUsagePercentage = errorTokenSession.TokenUsagePercentage,
                    MaxTokens = errorTokenSession.MaxTokens
                },
                Session = new SessionInfo
                {
                    AgentThreadId = errorSessionId,
                    IsSessionClosed = false,
                    SessionMessage = string.Empty
                }
            };
        }
        finally
        {
          //  await thread.DeleteAsync();
        }
    }

    /// <summary>
    /// Creates a new Azure Agent Thread or retrieves an existing one based on the provided thread ID
    /// </summary>
    /// <param name="agentThreadId">Optional existing thread ID. If null, creates a new thread</param>
    /// <returns>AzureAIAgentThread instance</returns>
    /// <exception cref="ArgumentException">Thrown when thread ID format is invalid</exception>
    private AzureAIAgentThread CreateOrGetThread(string? agentThreadId)
    {
        if (!string.IsNullOrEmpty(agentThreadId))
        {
            // Validate thread ID format
            if (!agentThreadId.StartsWith("thread_"))
            {
                throw new ArgumentException($"Invalid thread ID format. Expected format: 'thread_xxxxx', but received: '{agentThreadId}'");
            }
            
            // Create thread with existing ID
            return new AzureAIAgentThread(client: masterAgent.Client, id: agentThreadId);
        }
        else
        {
            // Create new thread
            return new AzureAIAgentThread(client: masterAgent.Client);
        }
    }



    private int ExtractTotalTokenCount(ChatMessageContent response)
    {
        if (response.Metadata?.TryGetValue(USAGE_METADATA_KEY, out var usageObj) == true)
        {
            if (usageObj is OpenAI.Chat.ChatTokenUsage chatUsage)
            {
                return chatUsage.TotalTokenCount;
            }
            else if (usageObj is RunStepCompletionUsage runUsage)
            {
                var totalTokensProperty = runUsage.GetType().GetProperty(TOTAL_TOKENS_PROPERTY);
                if (totalTokensProperty != null)
                {
                    var totalTokens = totalTokensProperty.GetValue(runUsage);
                    if (totalTokens is int tokenCount)
                    {
                        return tokenCount;
                    }
                    else if (totalTokens is long longTokenCount)
                    {
                        return (int)longTokenCount;
                    }
                }
            }
        }
        
        return 0;
    }

    public async Task<bool> DeleteThreadAsync(string agentThreadId)
    {
        try
        {
            // Validate thread ID format
            if (!agentThreadId.StartsWith("thread_"))
            {
                throw new ArgumentException($"Invalid thread ID format. Expected format: 'thread_xxxxx', but received: '{agentThreadId}'");
            }

            // Create thread instance using the provided ID
            var thread = new AzureAIAgentThread(client: masterAgent.Client, id: agentThreadId);
            
            // Delete the thread
            await thread.DeleteAsync();
            
            // Also reset the token session for this thread
            _tokenSessionService.ResetSession(agentThreadId);
            
            Console.WriteLine($"Thread {agentThreadId} deleted successfully");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting thread {agentThreadId}: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return false;
        }
    }
}