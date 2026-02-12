namespace PlanMorph.Application.DTOs.Order;

public class OrderFileDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
}
