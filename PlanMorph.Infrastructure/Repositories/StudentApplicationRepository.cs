using Microsoft.EntityFrameworkCore;
using PlanMorph.Core.Entities.Mentorship;
using PlanMorph.Core.Interfaces.Repositories;
using PlanMorph.Infrastructure.Data;

namespace PlanMorph.Infrastructure.Repositories;

public class StudentApplicationRepository : Repository<StudentApplication>, IStudentApplicationRepository
{
    public StudentApplicationRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<StudentApplication>> GetPendingApplicationsAsync()
        => await _context.StudentApplications
            .Where(a => a.Status == ApplicationStatus.Pending)
            .Include(a => a.User)
            .Include(a => a.InvitedByMentor)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<StudentApplication>> GetApplicationsByStatusAsync(ApplicationStatus status)
        => await _context.StudentApplications
            .Where(a => a.Status == status)
            .Include(a => a.User)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

    public async Task<StudentApplication?> GetApplicationByUserIdAsync(Guid userId)
        => await _context.StudentApplications
            .Include(a => a.User)
            .Include(a => a.InvitedByMentor)
            .FirstOrDefaultAsync(a => a.UserId == userId);

    public async Task<IEnumerable<StudentApplication>> GetApplicationsByMentorInviteAsync(Guid mentorId)
        => await _context.StudentApplications
            .Where(a => a.InvitedByMentorId == mentorId)
            .Include(a => a.User)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

    public async Task<bool> HasPendingApplicationAsync(Guid userId)
        => await _context.StudentApplications
            .AnyAsync(a => a.UserId == userId &&
                          (a.Status == ApplicationStatus.Pending || a.Status == ApplicationStatus.UnderReview));
}
