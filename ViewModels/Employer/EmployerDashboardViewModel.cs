namespace JobSeeker.ViewModels.Employer
{
    public class EmployerDashboardViewModel
    {
        public int TotalCandidates { get; set; }
        public int ProfilesViewedCount { get; set; }
        public int AvailableCandidatesCount { get; set; }
        public List<CandidateResultViewModel> RecentCandidates { get; set; } = new List<CandidateResultViewModel>();
    }
}
