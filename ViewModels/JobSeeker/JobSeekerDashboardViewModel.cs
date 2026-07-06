namespace JobSeeker.ViewModels.JobSeeker
{
    public class JobSeekerDashboardViewModel
    {
        public int ProfileCompletionPercentage { get; set; }
        public int ProfileViewsCount { get; set; }
        public int UnreadNotificationsCount { get; set; }
        public bool HasResume { get; set; }
        public string? ResumeOriginalName { get; set; }
        public DateTime? ResumeUploadedAt { get; set; }
        public List<RecentViewerViewModel> RecentViewers { get; set; } = new List<RecentViewerViewModel>();
        public List<NotificationViewModel> RecentNotifications { get; set; } = new List<NotificationViewModel>();
        public List<string> CompletionTips { get; set; } = new List<string>();
    }

    public class RecentViewerViewModel
    {
        public string CompanyName { get; set; } = string.Empty;
        public string? LogoFileName { get; set; }
        public DateTime ViewedAt { get; set; }
    }
}
