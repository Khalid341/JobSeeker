namespace JobSeeker.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalEmployers { get; set; }
        public int TotalJobSeekers { get; set; }
        public int TotalResumes { get; set; }
        public int TotalProfileViews { get; set; }
        public List<RecentUserViewModel> RecentUsers { get; set; } = new List<RecentUserViewModel>();
        public List<DailyViewChartViewModel> ViewsChartData { get; set; } = new List<DailyViewChartViewModel>();
    }

    public class RecentUserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class DailyViewChartViewModel
    {
        public string Date { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
