namespace PortfolioAPI.Models;

public class CvFile
{
    public int Id { get; set; }
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public required byte[] Data { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
