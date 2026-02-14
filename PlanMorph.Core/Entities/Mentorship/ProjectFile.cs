using PlanMorph.Core.Entities;

namespace PlanMorph.Core.Entities.Mentorship;

public class ProjectFile : BaseEntity
{
    public Guid? IterationId { get; set; }
    public ProjectIteration? Iteration { get; set; }

    public Guid ProjectId { get; set; }
    public MentorshipProject Project { get; set; } = null!;

    public string FileName { get; set; } = string.Empty;
    public FileType FileType { get; set; }
    public ProjectFileCategory Category { get; set; }
    public string StorageUrl { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public int Version { get; set; } = 1;

    public Guid UploadedById { get; set; }
    public User UploadedBy { get; set; } = null!;
}

public enum ProjectFileCategory
{
    ArchitecturalDrawing,
    StructuralDrawing,
    FloorPlan,
    Render,
    BOQ,
    Reference,
    Other
}
