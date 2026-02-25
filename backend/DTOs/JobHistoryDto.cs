namespace PortfolioAPI.DTOs;

public record JobHistoryDto(
    string Company,
    string Role,
    string? Description,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsCurrentRole,
    int Order
);
