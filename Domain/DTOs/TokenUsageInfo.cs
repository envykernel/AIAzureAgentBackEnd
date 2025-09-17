namespace Domain.DTOs;

public class TokenUsageInfo
{
    public int TokenCount { get; set; } = 0;
    public int TotalTokenCount { get; set; } = 0;
    public int MaxTokens { get; set; } = 0;
    public int RemainingTokens { get; set; } = 0;
    public double TokenUsagePercentage { get; set; } = 0.0;
}
