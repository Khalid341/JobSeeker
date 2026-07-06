namespace JobSeeker.ViewModels.Admin
{
    public class EmployerListViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? Industry { get; set; }
        public string? City { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public int JobPostsCount { get; set; }
    }
}
