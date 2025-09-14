using Domain.DTOs;

namespace Domain.Interfaces;

public interface IChatCommandHandler
{
    Task<ChatResponse> HandleAsync(ChatRequest request);
}
