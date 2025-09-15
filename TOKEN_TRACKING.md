# Token Tracking Implementation

## Overview

This implementation provides in-memory token accumulation without database persistence. Each request calculates and returns token usage information including:

- `TokenCount`: Tokens used in the current request
- `TotalTokenCount`: Accumulated tokens for the session/thread
- `RemainingTokens`: Tokens remaining before hitting the limit
- `TokenUsagePercentage`: Percentage of token limit used

## Key Components

### 1. TokenTrackingService
- **Location**: `Domain/Services/TokenTrackingService.cs`
- **Purpose**: Manages token accumulation in memory using `ConcurrentDictionary`
- **Lifetime**: Singleton (maintains state across requests)
- **Thread-Safe**: Uses `ConcurrentDictionary` for thread safety

### 2. TokenSession
- **Location**: `Domain/Services/TokenTrackingService.cs`
- **Purpose**: Represents token usage for a single session/thread
- **Properties**:
  - `SessionId`: Unique identifier (typically AgentThreadId)
  - `TotalTokenCount`: Accumulated tokens
  - `MaxTokensPerSession`: Configured limit
  - `RemainingTokens`: Calculated remaining tokens
  - `TokenUsagePercentage`: Usage percentage
  - `LastUpdated`: Timestamp of last update

### 3. Configuration
- **Location**: `Domain/Configuration/AzureConfiguration.cs`
- **Settings**:
  - `MaxTokensPerSession`: Maximum tokens per session (default: 100,000)
  - `WarningThresholdPercentage`: Warning threshold (default: 90%)
  - `AutoResetOnLimitExceeded`: Auto-reset behavior (default: false)

## Usage Examples

### Basic Usage
```csharp
// Each request automatically tracks tokens
var response = await chatService.GenerateAgentResponseAsync("Hello", threadId);

// Response now includes:
// - response.TokenCount (current request)
// - response.TotalTokenCount (accumulated)
// - response.RemainingTokens (remaining)
// - response.TokenUsagePercentage (usage %)
```

### Check Token Session
```csharp
// Get current token session without making a new request
var tokenSession = chatService.GetTokenSession(threadId);
if (tokenSession != null)
{
    Console.WriteLine($"Total tokens used: {tokenSession.TotalTokenCount}");
    Console.WriteLine($"Remaining tokens: {tokenSession.RemainingTokens}");
    Console.WriteLine($"Usage percentage: {tokenSession.TokenUsagePercentage}%");
}
```

### Reset Token Session
```csharp
// Reset token usage for a specific thread
chatService.ResetTokenSession(threadId);
```

## Configuration

### appsettings.json
```json
{
  "Azure": {
    "TokenLimits": {
      "MaxTokensPerSession": 100000,
      "WarningThresholdPercentage": 90.0,
      "AutoResetOnLimitExceeded": false
    }
  }
}
```

## Benefits

1. **No Database Required**: Uses in-memory storage
2. **Thread-Safe**: ConcurrentDictionary ensures thread safety
3. **Configurable**: Token limits and thresholds are configurable
4. **Session-Based**: Each AgentThreadId maintains its own token count
5. **Real-Time**: Immediate token tracking and reporting
6. **Memory Efficient**: Only stores essential token data

## Considerations

1. **Memory Usage**: Token sessions persist until application restart or manual cleanup
2. **Scalability**: For high-traffic applications, consider implementing cleanup policies
3. **Persistence**: Data is lost on application restart (by design)
4. **Session Management**: Sessions are identified by AgentThreadId

## API Response Format

```json
{
  "content": "Agent response content",
  "tokenCount": 150,
  "totalTokenCount": 1250,
  "remainingTokens": 98750,
  "tokenUsagePercentage": 1.25,
  "agentThreadId": "thread-123"
}
```

## Monitoring

The service logs token usage information:
```
Token usage - Current: 150, Total: 1250, Remaining: 98750, Usage: 1.25%
```

This helps monitor token consumption patterns and identify when sessions are approaching limits.
