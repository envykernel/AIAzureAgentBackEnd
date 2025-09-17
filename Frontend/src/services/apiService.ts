// API service for communicating with the .NET backend
export interface ChatRequest {
  message: string;
  agentThreadId?: string;
}

export interface ChatResponse {
  message: string;
  role: string;
  timestamp: string;
  tokenUsage: {
    tokenCount: number;
    totalTokenCount: number;
    maxTokens: number;
    remainingTokens: number;
    tokenUsagePercentage: number;
  };
  session: {
    agentThreadId: string;
    isSessionClosed: boolean;
    sessionMessage: string;
  };
  isError: boolean;
  errorType: string;
  errorMessage: string;
}

export interface ApiError {
  error: string;
  message: string;
  details?: string;
  agentThreadId?: string;
  currentTokenCount?: number;
  maxTokens?: number;
}

class ApiService {
  private baseUrl: string;

  constructor() {
    // In development, the API runs on port 5107
    // In production, this would be the same domain
    this.baseUrl = import.meta.env.DEV 
      ? 'http://localhost:5107' 
      : window.location.origin;
  }

  async sendMessage(request: ChatRequest): Promise<ChatResponse> {
    try {
      const response = await fetch(`${this.baseUrl}/api/agent/chat`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(request),
      });

      if (!response.ok) {
        const errorData: ApiError = await response.json();
        // Create a custom error that includes the structured error data
        const error = new Error(errorData.message || `HTTP error! status: ${response.status}`);
        (error as any).apiError = errorData;
        throw error;
      }

      return await response.json();
    } catch (error) {
      if (error instanceof Error) {
        throw error;
      }
      throw new Error('An unexpected error occurred');
    }
  }

  async deleteThread(agentThreadId: string): Promise<{ message: string; agentThreadId: string }> {
    try {
      const response = await fetch(`${this.baseUrl}/api/agent/threads/${agentThreadId}`, {
        method: 'DELETE',
        headers: {
          'Content-Type': 'application/json',
        },
      });

      if (!response.ok) {
        const errorData: ApiError = await response.json();
        throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
      }

      return await response.json();
    } catch (error) {
      if (error instanceof Error) {
        throw error;
      }
      throw new Error('An unexpected error occurred while deleting thread');
    }
  }
}

export const apiService = new ApiService();
