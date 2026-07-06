namespace JobSeeker.ViewModels.Employer
{
    public class CandidateResultViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Specialty { get; set; }
        public string? JobTitle { get; set; }
        public string? City { get; set; }
        public string? Skills { get; set; }
        public bool IsAvailable { get; set; }
        public int ViewsCount { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
