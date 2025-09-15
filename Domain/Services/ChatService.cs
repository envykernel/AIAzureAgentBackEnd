using Domain.Interfaces;
using Domain.Configuration;
using Domain.DTOs;
using Domain.Exceptions;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Azure.AI.Agents.Persistent;
using Microsoft.SemanticKernel.Agents;


namespace Domain.Services;

public class ChatService : IChatService
{
    private const string USAGE_METADATA_KEY = "Usage";
    private const string TOTAL_TOKENS_PROPERTY = "TotalTokens";
    
    private readonly IAzureAgentFactory _azureAgentFactory;
    private readonly AzureAIAgent masterAgent;
    private readonly Kernel _kernel;
    private readonly IConfigurationValidationService _configurationValidationService;

     private const string TranslatorName = "NumeroTranslator";
    private const string TranslatorInstructions = "Add one to latest user number and spell it in spanish without explanation.";


    public ChatService(
        IAzureAgentFactory azureAgentFactory,
        AzureConfiguration azureConfiguration,
        IKernelFactory kernelFactory,
        IConfigurationValidationService configurationValidationService)
    {
        _azureAgentFactory = azureAgentFactory;
        _kernel = kernelFactory.CreateKernel();
        _configurationValidationService = configurationValidationService;
        
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
        AzureAIAgentThread thread;


        if(!string.IsNullOrEmpty(agentThreadId))
        {
          // Get messages from azure agent thread
          var messages =  masterAgent.Client.Messages.GetMessages(threadId: agentThreadId, order:ListSortOrder.Ascending);
          
          // Create ThreadMessageOptions from agent thread messages
          var threadMessages = CreateThreadMessageOptionsFromAgentMessages(messages);
          
          foreach(var message in messages)
          {
            foreach(var messageContent in message.ContentItems)
            {
               switch(messageContent)
               {
                case MessageTextContent textTem:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"[{message.Role}] {textTem.Text}");
                    Console.ResetColor();
                    break;
                
               }
            }
          }

         thread = new AzureAIAgentThread(client: masterAgent.Client, messages: threadMessages);
        }
        else
        {
         thread = new AzureAIAgentThread(client: masterAgent.Client);
        }


        try
        {
            Console.WriteLine($"Generating agent response for message: {userMessageContent}");
            
            await foreach (ChatMessageContent response in masterAgent.InvokeAsync(new ChatMessageContent(AuthorRole.User, userMessageContent), thread))
            {

                var tokenCount = ExtractTotalTokenCount(response);
                
                Console.WriteLine($"Agent response received: {response.Content}");
                return new AgentResponse
                {
                    Content = response.Content ?? "No content in response.",
                    TokenCount = tokenCount,
                    AgentThreadId = thread.Id ?? string.Empty
                };
            }
            
            return new AgentResponse
            {
                Content = "No response generated from agent.",
                TokenCount = 0,
                AgentThreadId = thread.Id ?? string.Empty
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GenerateAgentResponseAsync: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            // Log the exception in production
            return new AgentResponse
            {
                Content = $"I encountered an error while processing your request: {ex.Message}",
                TokenCount = 0,
                AgentThreadId = thread?.Id ?? string.Empty
            };
        }
        finally
        {
          //  await thread.DeleteAsync();
        }
    }


    public int EstimateTokenCount(string text)
    {
        // Simple token estimation (roughly 4 characters per token)
        // In production, use a proper tokenizer
        return Math.Max(1, text.Length / 4);
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

     private ChatCompletionAgent CreateTruncatingAgent(int reducerMessageCount, int reducerThresholdCount, bool useChatClient) 
     {
        return new()
        {
            Name = TranslatorName,
            Instructions = TranslatorInstructions,
            Kernel = _kernel,
            HistoryReducer = new ChatHistoryTruncationReducer(reducerMessageCount, reducerThresholdCount),
        };
     }

     private List<ThreadMessageOptions> ConvertChatHistoryToThreadMessageOptions(IEnumerable<ChatMessageContent> messages)
     {
        var threadMessages = new List<ThreadMessageOptions>();
        
        foreach (var message in messages)
        {
            Console.WriteLine($"Message: {message.Content} - Role: {message.Role}");
            var role = message.Role.ToString() switch
            {
                "User" => Azure.AI.Agents.Persistent.MessageRole.User,
                "Agent" => Azure.AI.Agents.Persistent.MessageRole.Agent,
                "System" => Azure.AI.Agents.Persistent.MessageRole.Agent,
                _ => Azure.AI.Agents.Persistent.MessageRole.Agent
            };
            
            threadMessages.Add(new ThreadMessageOptions(role: role, content: message.Content));
        }
        
        return threadMessages;
     }

    private List<ThreadMessageOptions> CreateThreadMessageOptionsFromAgentMessages(IEnumerable<Azure.AI.Agents.Persistent.PersistentThreadMessage> messages)
    {
        var threadMessages = new List<ThreadMessageOptions>();
        
        foreach (var message in messages)
        {
            // Extract text content from message content items
            string content = string.Empty;
            foreach (var contentItem in message.ContentItems)
            {
                if (contentItem is MessageTextContent textContent)
                {
                    content = textContent.Text;
                    break;
                }
            }
            
            // Map the role from agent thread message to ThreadMessageOptions role
            var role = message.Role.ToString() switch
            {
                "user" => Azure.AI.Agents.Persistent.MessageRole.User,
                "assistant" => Azure.AI.Agents.Persistent.MessageRole.Agent,
                _ => Azure.AI.Agents.Persistent.MessageRole.Agent
            };
            
            var threadMessage = new ThreadMessageOptions(role: role, content: content);
            threadMessages.Add(threadMessage);
        }
        
        return threadMessages;
    }
       

}