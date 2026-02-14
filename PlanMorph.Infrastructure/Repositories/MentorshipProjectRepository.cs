using Microsoft.EntityFrameworkCore;
using PlanMorph.Core.Entities.Mentorship;
using PlanMorph.Core.Interfaces.Repositories;
using PlanMorph.Infrastructure.Data;

namespace PlanMorph.Infrastructure.Repositories;

public class MentorshipProjectRepository : Repository<MentorshipProject>, IMentorshipProjectRepository
{
    public MentorshipProjectRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<MentorshipProject>> GetProjectsByClientIdAsync(Guid clientId)
        => await _context.MentorshipProjects
            .Where(p => p.ClientId == clientId)
            .Include(p => p.Iterations)
            .Include(p => p.Deliverables)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<MentorshipProject>> GetProjectsByMentorIdAsync(Guid mentorId)
        => await _context.MentorshipProjects
            .Where(p => p.MentorId == mentorId)
            .Include(p => p.Iterations)
            .Include(p => p.Student)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<MentorshipProject>> GetProjectsByStudentIdAsync(Guid studentId)
        => await _context.MentorshipProjects
            .Where(p => p.StudentId == studentId)
            .Include(p => p.Iterations)
            .Include(p => p.Mentor)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<MentorshipProject>> GetProjectsByStatusAsync(ProjectStatus status)
        => await _context.MentorshipProjects
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<MentorshipProject>> GetPublishedProjectsAsync()
        => await _context.MentorshipProjects
            .Where(p => p.Status == ProjectStatus.Published)
            .Include(p => p.Client)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public async Task<MentorshipProject?> GetProjectWithDetailsAsync(Guid projectId)
        => await _context.MentorshipProjects
            .Include(p => p.Client)
            .Include(p => p.Mentor)
            .Include(p => p.Student)
            .Include(p => p.Iterations.OrderByDescending(i => i.IterationNumber))
                .ThenInclude(i => i.Files)
            .Include(p => p.Messages.OrderBy(m => m.CreatedAt))
            .Include(p => p.Deliverables.OrderByDescending(d => d.DeliveryNumber))
            .Include(p => p.AuditLogs.OrderByDescending(a => a.CreatedAt))
            .Include(p => p.Dispute)
            .FirstOrDefaultAsync(p => p.Id == projectId);

    public async Task<IEnumerable<MentorshipProject>> SearchProjectsAsync(string searchTerm)
        => await _context.MentorshipProjects
            .Where(p => p.Title.Contains(searchTerm) ||
                        p.Description.Contains(searchTerm) ||
                        p.ProjectNumber.Contains(searchTerm))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public async Task<int> GetActiveProjectCountByMentorAsync(Guid mentorId)
        => await _context.MentorshipProjects
            .CountAsync(p => p.MentorId == mentorId &&
                            p.Status != ProjectStatus.Completed &&
                            p.Status != ProjectStatus.Paid &&
                            p.Status != ProjectStatus.Cancelled);

    public async Task<int> GetActiveProjectCountByStudentAsync(Guid studentId)
        => await _context.MentorshipProjects
            .CountAsync(p => p.StudentId == studentId &&
                            p.Status != ProjectStatus.Completed &&
                            p.Status != ProjectStatus.Paid &&
                            p.Status != ProjectStatus.Cancelled);
}
