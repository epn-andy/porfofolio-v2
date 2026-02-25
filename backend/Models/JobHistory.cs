namespace PortfolioAPI.Models;

public class JobHistory
{
    public int Id { get; set; }
    public required string Company { get; set; }
    public required string Role { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrentRole { get; set; } = false;
    public int Order { get; set; } = 0;
}
