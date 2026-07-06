namespace JobSeeker.ViewModels.Admin
{
    public class JobSeekerListViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Specialty { get; set; }
        public string? JobTitle { get; set; }
        public string? City { get; set; }
        public bool IsAvailable { get; set; }
        public bool HasResume { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public int ViewsCount { get; set; }
    }
}
