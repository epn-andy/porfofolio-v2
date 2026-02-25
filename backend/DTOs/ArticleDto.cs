namespace PortfolioAPI.DTOs;

public record ArticleDto(
    string Title,
    string Slug,
    string Content,
    string? Excerpt,
    bool Published
);
