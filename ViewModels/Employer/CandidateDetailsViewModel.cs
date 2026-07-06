namespace JobSeeker.ViewModels.Employer
{
    public class CandidateDetailsViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Specialty { get; set; }
        public string? JobTitle { get; set; }
        public string? Bio { get; set; }
        public string? Skills { get; set; }
        public string? City { get; set; }
        public string? PhoneNumber { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? GitHubUrl { get; set; }
        public bool IsAvailable { get; set; }
        public string? ResumeFileName { get; set; }
        public string? ResumeOriginalName { get; set; }
        public DateTime? ResumeUploadedAt { get; set; }
        public bool HasResume { get; set; }
        public int ViewsCount { get; set; }
        public bool AlreadyViewedToday { get; set; }
    }
}
