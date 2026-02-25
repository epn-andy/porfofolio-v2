namespace PortfolioAPI.DTOs;

public record ProjectDto(
    string Title,
    string Description,
    string? TechStack,
    string? LiveUrl,
    string? GithubUrl,
    string? ImageUrl,
    int Order
);
