using System.ComponentModel.DataAnnotations;

namespace JobSeeker.ViewModels.Employer
{
    public class EmployerProfileViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        [Display(Name = "اسم الشركة")]
        public string CompanyName { get; set; } = string.Empty;

        [Display(Name = "المجال")]
        public string? Industry { get; set; }

        [Display(Name = "حجم الشركة")]
        public string? CompanySize { get; set; }

        [Display(Name = "الموقع الإلكتروني")]
        public string? Website { get; set; }

        [Display(Name = "وصف الشركة")]
        public string? Description { get; set; }

        [Display(Name = "المدينة")]
        public string? City { get; set; }

        [Display(Name = "شعار الشركة")]
        public string? LogoFileName { get; set; }

        [Display(Name = "تاريخ الانضمام")]
        public DateTime CreatedAt { get; set; }

        public int JobPostsCount { get; set; }
        public int ProfileViewsCount { get; set; }
    }
}
