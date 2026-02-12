namespace PlanMorph.Core.Entities;

public class DesignFile : BaseEntity
{
    public Guid DesignId { get; set; }
    public Design Design { get; set; } = null!;
    
    public string FileName { get; set; } = string.Empty;
    public FileType FileType { get; set; }
    public FileCategory Category { get; set; }
    public string StorageUrl { get; set; } = string.Empty; // URL in DigitalOcean Spaces
    public long FileSizeBytes { get; set; }
    public bool IsWatermarked { get; set; }
    public string? ThumbnailUrl { get; set; }
}

public enum FileType
{
    Image,      // JPG, PNG
    Video,      // MP4
    PDF,        // Drawings, BOQ
    CAD         // DWG files
}

public enum FileCategory
{
    // Preview files (watermarked, public)
    PreviewImage,
    PreviewVideo,
    
    // Purchased files (no watermark, secured)
    ArchitecturalDrawing,
    StructuralDrawing,
    BOQ,
    FullRenderImage,
    FullRenderVideo
}