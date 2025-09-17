# AI Agent Chat Interface

A modern React frontend application for testing the .NET AI Agent API with comprehensive token tracking and statistics.

## Features

- **Real-time Chat Interface**: Clean, modern chat UI with Material-UI components
- **Token Statistics**: Real-time display of token usage, remaining tokens, and usage percentage
- **Session Management**: Automatic thread management with session reset functionality
- **Error Handling**: Comprehensive error handling with user-friendly messages
- **Responsive Design**: Mobile-friendly interface that works on all screen sizes
- **API Integration**: Seamless communication with the .NET backend API

## Interface Layout

The application is structured as requested:

- **Header (10% height)**: Contains the application title and control buttons
- **Body (90% height)**: 
  - Token statistics panel (when active)
  - Chat messages area (scrollable)
  - Message input form
  - Error display area

## Token Statistics Display

For each message, the interface shows:
- **Token Count**: Number of tokens used for the current message
- **Total Token Count**: Cumulative tokens used in the session
- **Remaining Tokens**: Tokens remaining before hitting limits
- **Usage Percentage**: Percentage of token limit used
- **Thread ID**: Current agent thread identifier

## API Endpoints Used

- `POST /api/agent/chat` - Send messages to the AI agent
- `POST /api/agent/tokens/{agentThreadId}/reset` - Reset token session

## Getting Started

1. **Start the .NET API**:
   ```bash
   cd AgentApi
   dotnet run
   ```

2. **Start the Frontend**:
   ```bash
   cd Frontend
   npm install
   npm run dev
   ```

3. **Open the Application**:
   Navigate to `http://localhost:5173` in your browser

## Usage

1. **Start Chatting**: Type a message in the input field and press Enter or click Send
2. **View Statistics**: Token usage statistics appear automatically for each response
3. **Reset Session**: Click the refresh button in the header to start a new session
4. **Monitor Usage**: Keep track of token consumption to avoid hitting limits

## Error Handling

The interface handles various error scenarios:
- **Network Errors**: Connection issues with the API
- **Token Limit Exceeded**: When token limits are reached
- **Configuration Errors**: Azure configuration issues
- **Invalid Requests**: Malformed requests or parameters

## Styling

The interface uses Material-UI components with:
- **Primary Colors**: Blue theme for user messages
- **Secondary Colors**: Gray theme for assistant messages
- **Status Colors**: Color-coded token usage (green/yellow/red)
- **Responsive Layout**: Adapts to different screen sizes

## Development Notes

- The API service automatically detects development vs production environments
- Token statistics are color-coded based on usage levels
- Messages are automatically scrolled to the bottom
- Session state is maintained throughout the conversation
- Error messages are dismissible and user-friendly
