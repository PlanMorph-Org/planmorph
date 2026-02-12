namespace PlanMorph.Application.DTOs.Design;

public class DesignDetailDto : DesignDto
{
    public string ArchitectName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<DesignFileDto> Files { get; set; } = new();
}

public class DesignFileDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public long FileSizeBytes { get; set; }
}