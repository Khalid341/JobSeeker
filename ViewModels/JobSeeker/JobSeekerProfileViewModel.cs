using System.ComponentModel.DataAnnotations;

namespace JobSeeker.ViewModels.JobSeeker
{
    public class JobSeekerProfileViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        [Display(Name = "التخصص")]
        public string? Specialty { get; set; }

        [Display(Name = "العنوان المهني")]
        public string? JobTitle { get; set; }

        [Display(Name = "نبذة شخصية")]
        public string? Bio { get; set; }

        [Display(Name = "المهارات")]
        public string? Skills { get; set; }

        [Display(Name = "المدينة")]
        public string? City { get; set; }

        [Display(Name = "رقم الجوال")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "رابط LinkedIn")]
        public string? LinkedInUrl { get; set; }

        [Display(Name = "رابط GitHub")]
        public string? GitHubUrl { get; set; }

        [Display(Name = "اسم ملف السيرة الذاتية")]
        public string? ResumeFileName { get; set; }

        [Display(Name = "الاسم الأصلي للملف")]
        public string? ResumeOriginalName { get; set; }

        [Display(Name = "تاريخ رفع السيرة")]
        public DateTime? ResumeUploadedAt { get; set; }

        [Display(Name = "متاح للعمل")]
        public bool IsAvailable { get; set; }

        [Display(Name = "تاريخ الانضمام")]
        public DateTime CreatedAt { get; set; }

        public int ProfileViewsCount { get; set; }
        public int ApplicationsCount { get; set; }
    }
}
