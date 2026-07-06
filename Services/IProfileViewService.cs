using JobSeeker.Models;

namespace JobSeeker.Services
{
    public interface IProfileViewService
    {
        Task<ProfileView?> RecordViewAsync(int employerProfileId, int jobSeekerProfileId);
        Task<bool> HasViewedInLast24HoursAsync(int employerProfileId, int jobSeekerProfileId);
        Task<int> GetViewsCountAsync(int jobSeekerProfileId);
        Task<List<ProfileView>> GetRecentViewersAsync(int jobSeekerProfileId, int count = 5);
    }
}
