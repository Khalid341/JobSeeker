using JobSeeker.Data;
using JobSeeker.Models;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.Services
{
    public class ProfileViewService : IProfileViewService
    {
        private readonly ApplicationDbContext _context;

        public ProfileViewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProfileView?> RecordViewAsync(int employerProfileId, int jobSeekerProfileId)
        {
            // Prevent duplicate views within 24 hours
            if (await HasViewedInLast24HoursAsync(employerProfileId, jobSeekerProfileId))
            {
                return null;
            }

            var profileView = new ProfileView
            {
                EmployerProfileId = employerProfileId,
                JobSeekerProfileId = jobSeekerProfileId,
                ViewedAt = DateTime.UtcNow,
                NotificationSent = false
            };

            _context.ProfileViews.Add(profileView);
            await _context.SaveChangesAsync();

            return profileView;
        }

        public async Task<bool> HasViewedInLast24HoursAsync(int employerProfileId, int jobSeekerProfileId)
        {
            var last24Hours = DateTime.UtcNow.AddHours(-24);
            return await _context.ProfileViews
                .AnyAsync(p => p.EmployerProfileId == employerProfileId
                    && p.JobSeekerProfileId == jobSeekerProfileId
                    && p.ViewedAt >= last24Hours);
        }

        public async Task<int> GetViewsCountAsync(int jobSeekerProfileId)
        {
            return await _context.ProfileViews
                .CountAsync(p => p.JobSeekerProfileId == jobSeekerProfileId);
        }

        public async Task<List<ProfileView>> GetRecentViewersAsync(int jobSeekerProfileId, int count = 5)
        {
            return await _context.ProfileViews
                .Where(p => p.JobSeekerProfileId == jobSeekerProfileId)
                .Include(p => p.EmployerProfile)
                .ThenInclude(e => e.User)
                .OrderByDescending(p => p.ViewedAt)
                .Take(count)
                .ToListAsync();
        }
    }
}
