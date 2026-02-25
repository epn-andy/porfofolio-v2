namespace PortfolioAPI.Models;

public class Project
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public string? TechStack { get; set; }
    public string? LiveUrl { get; set; }
    public string? GithubUrl { get; set; }
    public string? ImageUrl { get; set; }
    public int Order { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
