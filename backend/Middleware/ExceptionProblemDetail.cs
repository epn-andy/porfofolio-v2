namespace PortfolioAPI.Middleware;

public class ExceptionProblemDetail
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public int Status { get; init; }
    public string Type { get; init; } = string.Empty;
    public string? Detail { get; init; }
    public Dictionary<string, string[]>? Errors { get; init; }
}
