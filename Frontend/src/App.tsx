import React, { useState, useRef, useEffect } from 'react';
import {
  AppBar,
  Toolbar,
  Typography,
  Container,
  Paper,
  TextField,
  Button,
  Box,
  Chip,
  CircularProgress,
  Alert,
  IconButton,
  Tooltip,
  Avatar,
  Divider,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Snackbar
} from '@mui/material';
import {
  Send as SendIcon,
  Refresh as RefreshIcon,
  Settings as SettingsIcon,
  Person as PersonIcon,
  SmartToy as BotIcon,
  Token as TokenIcon,
  TrendingUp as TrendingUpIcon,
  Percent as PercentIcon,
  CheckCircle as CheckCircleIcon,
  Close as CloseIcon,
  ContentCopy as CopyIcon
} from '@mui/icons-material';
import { styled } from '@mui/material/styles';
import { apiService } from './services/apiService';
import './App.css';

// Types for API responses
interface ChatRequest {
  message: string;
  agentThreadId?: string;
}

interface ChatResponse {
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

interface Message {
  id: string;
  content: string;
  role: 'user' | 'assistant';
  timestamp: Date;
  tokenCount?: number;
  totalTokenCount?: number;
  remainingTokens?: number;
  tokenUsagePercentage?: number;
  agentThreadId?: string;
}

// Styled components for modern design
const MainContainer = styled(Box)(({ theme }) => ({
  height: '100vh',
  display: 'flex',
  flexDirection: 'column',
  backgroundColor: '#f7f7f8',
}));

const Header = styled(AppBar)(({ theme }) => ({
  backgroundColor: '#ffffff',
  color: '#000000',
  boxShadow: '0 1px 3px rgba(0,0,0,0.1)',
  borderBottom: '1px solid #e5e5e5',
  flexShrink: 0,
}));

const HeaderToolbar = styled(Toolbar)(({ theme }) => ({
  justifyContent: 'space-between',
  padding: '12px 24px',
}));

const MessagesBody = styled(Box)(({ theme }) => ({
  flex: 1,
  overflowY: 'auto',
  padding: '24px',
  display: 'flex',
  flexDirection: 'column',
  gap: '24px',
  backgroundColor: '#f7f7f8',
}));

const MessageContainer = styled(Box)<{ isUser: boolean }>(({ theme, isUser }) => ({
  display: 'flex',
  justifyContent: isUser ? 'flex-end' : 'flex-start',
  alignItems: 'flex-start',
  gap: '12px',
  maxWidth: '100%',
  width: '100%',
}));

const MessageBubble = styled(Box)<{ isUser: boolean }>(({ theme, isUser }) => ({
  maxWidth: '70%',
  padding: '12px 16px',
  borderRadius: isUser ? '18px 18px 4px 18px' : '18px 18px 18px 4px',
  backgroundColor: isUser ? '#007bff' : '#ffffff',
  color: isUser ? '#ffffff' : '#000000',
  boxShadow: '0 1px 2px rgba(0,0,0,0.1)',
  border: isUser ? 'none' : '1px solid #e5e5e5',
  position: 'relative',
  wordWrap: 'break-word',
}));

const AvatarContainer = styled(Box)(({ theme }) => ({
  width: '32px',
  height: '32px',
  borderRadius: '50%',
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  flexShrink: 0,
}));

const Footer = styled(Box)(({ theme }) => ({
  flexShrink: 0,
  padding: '20px 24px 24px 24px',
  backgroundColor: '#ffffff',
  borderTop: '1px solid #e5e5e5',
}));

const InputContainer = styled(Box)(({ theme }) => ({
  display: 'flex',
  alignItems: 'flex-end',
  gap: '8px',
  width: '100%',
  padding: '12px 16px',
  backgroundColor: '#ffffff',
  borderRadius: '24px',
  border: '1px solid #d1d5db',
  boxShadow: '0 1px 3px rgba(0,0,0,0.1), 0 1px 2px rgba(0,0,0,0.06)',
  transition: 'all 0.2s ease',
  '&:focus-within': {
    borderColor: '#007bff',
    boxShadow: '0 4px 12px rgba(0,123,255,0.15), 0 2px 4px rgba(0,123,255,0.1)',
    transform: 'translateY(-1px)',
  },
}));

const TokenStatsBar = styled(Box)(({ theme }) => ({
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  gap: '24px',
  padding: '12px 24px',
  backgroundColor: 'rgba(255, 255, 255, 0.95)',
  backdropFilter: 'blur(10px)',
  borderBottom: '1px solid rgba(229, 229, 229, 0.5)',
  fontSize: '13px',
  fontWeight: 500,
  color: '#374151',
  position: 'sticky',
  top: 0,
  zIndex: 5,
}));

const StatItem = styled(Box)(({ theme }) => ({
  display: 'flex',
  alignItems: 'center',
  gap: '8px',
  padding: '6px 12px',
  backgroundColor: 'rgba(255, 255, 255, 0.8)',
  borderRadius: '20px',
  border: '1px solid rgba(229, 229, 229, 0.3)',
  boxShadow: '0 1px 3px rgba(0, 0, 0, 0.05)',
  transition: 'all 0.2s ease',
  '&:hover': {
    backgroundColor: 'rgba(255, 255, 255, 1)',
    boxShadow: '0 2px 8px rgba(0, 0, 0, 0.1)',
    transform: 'translateY(-1px)',
  },
}));

const StatIcon = styled(Box)(({ theme }) => ({
  width: '20px',
  height: '20px',
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  color: '#6b7280',
}));

const TokenLimitMessage = styled(Box)(({ theme }) => ({
  padding: '32px 24px',
  backgroundColor: '#ffffff',
  borderTop: '1px solid #e5e5e5',
  display: 'flex',
  flexDirection: 'column',
  alignItems: 'center',
  justifyContent: 'center',
  textAlign: 'center',
  gap: '16px',
}));

const SendButton = styled(Button)(({ theme }) => ({
  minWidth: '36px',
  width: '36px',
  height: '36px',
  borderRadius: '50%',
  backgroundColor: '#007bff',
  color: '#ffffff',
  boxShadow: '0 1px 2px rgba(0,0,0,0.1)',
  transition: 'all 0.2s ease',
  '&:hover': {
    backgroundColor: '#0056b3',
    transform: 'scale(1.05)',
    boxShadow: '0 2px 4px rgba(0,0,0,0.15)',
  },
  '&:disabled': {
    backgroundColor: '#e5e7eb',
    color: '#9ca3af',
    transform: 'none',
    boxShadow: 'none',
  },
  '&:active': {
    transform: 'scale(0.95)',
  },
}));

function App() {
  const [messages, setMessages] = useState<Message[]>([]);
  const [inputMessage, setInputMessage] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [agentThreadId, setAgentThreadId] = useState<string | null>(null);
  const [totalTokensUsed, setTotalTokensUsed] = useState(0);
  const [remainingTokens, setRemainingTokens] = useState(0);
  const [tokenUsagePercentage, setTokenUsagePercentage] = useState(0);
  const [isSessionClosed, setIsSessionClosed] = useState(false);
  const [sessionMessage, setSessionMessage] = useState('');
  const [isSettingsOpen, setIsSettingsOpen] = useState(false);
  const [settingsThreadId, setSettingsThreadId] = useState('');
  const [copySuccess, setCopySuccess] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  const sendMessage = async () => {
    if (!inputMessage.trim() || isLoading || isSessionClosed) return;

    const userMessage: Message = {
      id: Date.now().toString(),
      content: inputMessage,
      role: 'user',
      timestamp: new Date(),
    };

    setMessages(prev => [...prev, userMessage]);
    setInputMessage('');
    setIsLoading(true);
    setError(null);

    try {
      const request: ChatRequest = {
        message: inputMessage,
        agentThreadId: agentThreadId || undefined,
      };

      const data: ChatResponse = await apiService.sendMessage(request);

      const assistantMessage: Message = {
        id: (Date.now() + 1).toString(),
        content: data.message,
        role: 'assistant',
        timestamp: new Date(data.timestamp),
        tokenCount: data.tokenUsage.tokenCount,
        totalTokenCount: data.tokenUsage.totalTokenCount,
        remainingTokens: data.tokenUsage.remainingTokens,
        tokenUsagePercentage: data.tokenUsage.tokenUsagePercentage,
        agentThreadId: data.session.agentThreadId,
      };

      setMessages(prev => [...prev, assistantMessage]);
      setAgentThreadId(data.session.agentThreadId);
      setTotalTokensUsed(data.tokenUsage.totalTokenCount);
      setRemainingTokens(data.tokenUsage.remainingTokens);
      setTokenUsagePercentage(data.tokenUsage.tokenUsagePercentage);
      setIsSessionClosed(data.session.isSessionClosed);
      setSessionMessage(data.session.sessionMessage);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'An error occurred';
      setError(errorMessage);
      
      const errorMsg: Message = {
        id: (Date.now() + 1).toString(),
        content: `Error: ${errorMessage}`,
        role: 'assistant',
        timestamp: new Date(),
      };
      setMessages(prev => [...prev, errorMsg]);
    } finally {
      setIsLoading(false);
    }
  };

  const handleKeyPress = (event: React.KeyboardEvent) => {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      sendMessage();
    }
  };

  const resetSession = async () => {
    // Delete the existing thread on the server if one exists
    if (agentThreadId) {
      try {
        await apiService.deleteThread(agentThreadId);
      } catch (err) {
        console.warn('Failed to delete thread on server:', err);
      }
    }
    
    // Reset local state to start a new conversation
    setMessages([]);
    setAgentThreadId(null);
    setTotalTokensUsed(0);
    setRemainingTokens(0);
    setTokenUsagePercentage(0);
    setError(null);
    setIsSessionClosed(false);
    setSessionMessage('');
  };

  const formatTimestamp = (timestamp: Date) => {
    return timestamp.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  };

  const handleOpenSettings = () => {
    setSettingsThreadId(agentThreadId || '');
    setIsSettingsOpen(true);
  };

  const handleCloseSettings = () => {
    setIsSettingsOpen(false);
    setSettingsThreadId('');
  };

  const handleSaveThreadId = () => {
    if (settingsThreadId.trim()) {
      // Validate thread ID format
      const threadIdPattern = /^thread_[a-zA-Z0-9]+$/;
      if (!threadIdPattern.test(settingsThreadId.trim())) {
        setError('Invalid thread ID format. Expected format: thread_xxxxx');
        return;
      }
      setAgentThreadId(settingsThreadId.trim());
    } else {
      setAgentThreadId(null);
    }
    handleCloseSettings();
  };

  const handleCopyThreadId = async () => {
    if (agentThreadId) {
      try {
        await navigator.clipboard.writeText(agentThreadId);
        setCopySuccess(true);
      } catch (err) {
        console.error('Failed to copy thread ID:', err);
      }
    }
  };

  return (
    <MainContainer>
      {/* Header */}
      <Header position="static" elevation={0}>
        <HeaderToolbar>
          <Box display="flex" alignItems="center" gap={2}>
            <Typography variant="h6" sx={{ fontWeight: 600, color: '#000000' }}>
              AI Agent Chat
            </Typography>
          </Box>
          <Box display="flex" alignItems="center" gap={1}>
            <Tooltip title="Settings">
              <IconButton size="small" onClick={handleOpenSettings}>
                <SettingsIcon sx={{ color: '#666666' }} />
              </IconButton>
            </Tooltip>
            {agentThreadId && (
              <Tooltip title="New Chat">
                <IconButton size="small" onClick={resetSession}>
                  <RefreshIcon sx={{ color: '#666666' }} />
                </IconButton>
              </Tooltip>
            )}
          </Box>
        </HeaderToolbar>
      </Header>

      {/* Token Stats Bar */}
      {totalTokensUsed > 0 && (
        <TokenStatsBar>
          <StatItem>
            <StatIcon>
              <TokenIcon sx={{ fontSize: 16 }} />
            </StatIcon>
            <Typography variant="body2" sx={{ fontSize: '13px', fontWeight: 500 }}>
              {totalTokensUsed.toLocaleString()} used
            </Typography>
          </StatItem>
          
          <StatItem>
            <StatIcon>
              <TrendingUpIcon sx={{ fontSize: 16 }} />
            </StatIcon>
            <Typography variant="body2" sx={{ fontSize: '13px', fontWeight: 500 }}>
              {remainingTokens.toLocaleString()} left
            </Typography>
          </StatItem>
          
          <StatItem>
            <StatIcon>
              <PercentIcon 
                sx={{ 
                  fontSize: 16,
                  color: tokenUsagePercentage >= 90 ? '#ef4444' : tokenUsagePercentage >= 80 ? '#f59e0b' : '#10b981'
                }} 
              />
            </StatIcon>
            <Typography variant="body2" sx={{ fontSize: '13px', fontWeight: 500 }}>
              {tokenUsagePercentage.toFixed(1)}% usage
            </Typography>
          </StatItem>
          
          {isSessionClosed && (
            <StatItem sx={{ backgroundColor: '#f0f9ff', borderColor: '#bae6fd' }}>
              <StatIcon>
                <CheckCircleIcon sx={{ fontSize: 16, color: '#0369a1' }} />
              </StatIcon>
              <Typography variant="body2" sx={{ fontSize: '13px', fontWeight: 500, color: '#0369a1' }}>
                Session Closed
              </Typography>
            </StatItem>
          )}
        </TokenStatsBar>
      )}

      {/* Messages Body */}
      <MessagesBody>
          {messages.length === 0 ? (
            <Box 
              display="flex" 
              flexDirection="column"
              alignItems="center" 
              justifyContent="center" 
              height="100%"
              gap={2}
            >
              <BotIcon sx={{ fontSize: 64, color: '#007bff', opacity: 0.7 }} />
              <Typography variant="h5" color="textSecondary" sx={{ fontWeight: 500 }}>
                How can I help you today?
              </Typography>
              <Typography variant="body2" color="textSecondary" sx={{ opacity: 0.7 }}>
                Start a conversation by typing a message below
              </Typography>
            </Box>
          ) : (
            messages.map((message) => (
              <MessageContainer key={message.id} isUser={message.role === 'user'}>
                {message.role === 'assistant' && (
                  <AvatarContainer sx={{ backgroundColor: '#007bff' }}>
                    <BotIcon sx={{ fontSize: 18, color: '#ffffff' }} />
                  </AvatarContainer>
                )}
                
                <Box 
                  display="flex" 
                  flexDirection="column" 
                  gap={0.5} 
                  maxWidth="100%"
                  alignItems={message.role === 'user' ? 'flex-end' : 'flex-start'}
                  width="100%"
                >
                  <MessageBubble isUser={message.role === 'user'}>
                    <Typography 
                      variant="body1" 
                      sx={{ 
                        whiteSpace: 'pre-wrap', 
                        lineHeight: 1.5,
                        fontSize: '14px',
                        fontWeight: 400
                      }}
                    >
                      {message.content}
                    </Typography>
                  </MessageBubble>
                  
                  {message.tokenCount && (
                    <Box display="flex" justifyContent={message.role === 'user' ? 'flex-end' : 'flex-start'}>
                      <Chip 
                        size="small" 
                        label={`${message.tokenCount} tokens`} 
                        sx={{ 
                          fontSize: '10px',
                          height: '20px',
                          backgroundColor: 'rgba(0,0,0,0.05)',
                          color: '#666666'
                        }}
                      />
                    </Box>
                  )}
                </Box>

                {message.role === 'user' && (
                  <AvatarContainer sx={{ backgroundColor: '#6c757d' }}>
                    <PersonIcon sx={{ fontSize: 18, color: '#ffffff' }} />
                  </AvatarContainer>
                )}
              </MessageContainer>
            ))
          )}
          
          {isLoading && (
            <MessageContainer isUser={false}>
              <AvatarContainer sx={{ backgroundColor: '#007bff' }}>
                <BotIcon sx={{ fontSize: 18, color: '#ffffff' }} />
              </AvatarContainer>
              <Box display="flex" alignItems="center" gap={1} p={2} borderRadius={2} bgcolor="grey.100">
                <CircularProgress size={16} />
                <Typography variant="body2" color="text.secondary" sx={{ fontSize: '14px' }}>
                  AI Agent is thinking...
                </Typography>
              </Box>
            </MessageContainer>
          )}
          
          <div ref={messagesEndRef} />
      </MessagesBody>

      {/* Error Display */}
      {error && (
        <Box px={3} pb={2} sx={{ backgroundColor: '#f7f7f8' }}>
          <Alert severity="error" onClose={() => setError(null)}>
            {error}
          </Alert>
        </Box>
      )}

      {/* Footer */}
      {isSessionClosed ? (
        <Footer>
          <Box display="flex" alignItems="center" justifyContent="space-between" gap={3}>
            <Box display="flex" alignItems="center" gap={2}>
              <Box 
                sx={{ 
                  width: 48, 
                  height: 48, 
                  borderRadius: '50%', 
                  backgroundColor: '#f0f9ff', 
                  display: 'flex', 
                  alignItems: 'center', 
                  justifyContent: 'center' 
                }}
              >
                <CheckCircleIcon sx={{ fontSize: 24, color: '#0369a1' }} />
              </Box>
              <Box>
                <Typography variant="h6" sx={{ fontWeight: 600, color: '#0369a1', mb: 0.5 }}>
                  Session Complete
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ maxWidth: '400px' }}>
                  Your session has been closed because you've exceeded your token limit. Please create a new conversation to continue.
                </Typography>
              </Box>
            </Box>
            <Button
              variant="contained"
              onClick={resetSession}
              sx={{
                backgroundColor: '#007bff',
                color: '#ffffff',
                px: 3,
                py: 1.5,
                borderRadius: '24px',
                textTransform: 'none',
                fontWeight: 500,
                minWidth: '180px',
                '&:hover': {
                  backgroundColor: '#0056b3',
                },
              }}
            >
              Start New Conversation
            </Button>
          </Box>
        </Footer>
      ) : (
        <Footer>
          <InputContainer>
            <TextField
              fullWidth
              multiline
              maxRows={6}
              value={inputMessage}
              onChange={(e) => setInputMessage(e.target.value)}
              onKeyPress={handleKeyPress}
              placeholder="Message AI Agent..."
              disabled={isLoading}
              variant="standard"
              InputProps={{
                disableUnderline: true,
                sx: {
                  fontSize: '15px',
                  fontWeight: 400,
                  color: '#000000',
                  '& textarea': {
                    resize: 'none',
                    padding: '4px 0',
                    lineHeight: '1.5',
                  },
                  '& textarea::placeholder': {
                    color: '#9ca3af',
                    opacity: 1,
                  },
                },
              }}
              sx={{
                '& .MuiInputBase-root': {
                  padding: 0,
                  minHeight: '24px',
                },
                '& .MuiInputBase-input': {
                  padding: '4px 0',
                },
              }}
            />
            <SendButton
              onClick={sendMessage}
              disabled={!inputMessage.trim() || isLoading}
            >
              {isLoading ? <CircularProgress size={20} color="inherit" /> : <SendIcon />}
            </SendButton>
          </InputContainer>
        </Footer>
      )}
      
      {/* Settings Dialog */}
      <Dialog open={isSettingsOpen} onClose={handleCloseSettings} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
          <Typography variant="h6" sx={{ fontWeight: 600 }}>
            Settings
          </Typography>
          <IconButton onClick={handleCloseSettings} size="small">
            <CloseIcon />
          </IconButton>
        </DialogTitle>
        
        <DialogContent sx={{ pt: 2 }}>
          <Box sx={{ mb: 3 }}>
            <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 1, color: '#374151' }}>
              Current Thread ID
            </Typography>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
              <TextField
                fullWidth
                value={agentThreadId || 'No active thread'}
                disabled
                variant="outlined"
                size="small"
                sx={{
                  '& .MuiInputBase-input': {
                    fontFamily: 'monospace',
                    fontSize: '13px',
                    color: agentThreadId ? '#374151' : '#9ca3af'
                  }
                }}
              />
              {agentThreadId && (
                <Tooltip title="Copy Thread ID">
                  <IconButton onClick={handleCopyThreadId} size="small">
                    <CopyIcon sx={{ fontSize: 18 }} />
                  </IconButton>
                </Tooltip>
              )}
            </Box>
            
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              The Thread ID is used to maintain conversation continuity. You can manually set a specific thread ID to continue an existing conversation.
            </Typography>
          </Box>
          
          <Divider sx={{ my: 2 }} />
          
          <Box>
            <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 1, color: '#374151' }}>
              Set Custom Thread ID
            </Typography>
            <TextField
              fullWidth
              value={settingsThreadId}
              onChange={(e) => setSettingsThreadId(e.target.value)}
              placeholder="thread_xxxxx (leave empty for new conversation)"
              variant="outlined"
              size="small"
              sx={{
                '& .MuiInputBase-input': {
                  fontFamily: 'monospace',
                  fontSize: '13px'
                }
              }}
            />
            <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>
              Format: thread_ followed by alphanumeric characters (e.g., thread_2BoH4WFlxbW0PQhOCTz8aLT7)
            </Typography>
          </Box>
        </DialogContent>
        
        <DialogActions sx={{ p: 3, pt: 1 }}>
          <Button onClick={handleCloseSettings} sx={{ textTransform: 'none' }}>
            Cancel
          </Button>
          <Button 
            onClick={handleSaveThreadId} 
            variant="contained"
            sx={{ 
              textTransform: 'none',
              backgroundColor: '#007bff',
              '&:hover': { backgroundColor: '#0056b3' }
            }}
          >
            Save
          </Button>
        </DialogActions>
      </Dialog>
      
      {/* Copy Success Snackbar */}
      <Snackbar
        open={copySuccess}
        autoHideDuration={2000}
        onClose={() => setCopySuccess(false)}
        message="Thread ID copied to clipboard!"
      />
    </MainContainer>
  );
}

export default App;