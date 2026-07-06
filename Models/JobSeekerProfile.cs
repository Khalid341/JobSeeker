using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobSeeker.Models
{
    public class JobSeekerProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [StringLength(200)]
        [Display(Name = "التخصص")]
        public string? Specialty { get; set; }

        [StringLength(2000)]
        [Display(Name = "نبذة شخصية")]
        public string? Bio { get; set; }

        [StringLength(1000)]
        [Display(Name = "المهارات")]
        public string? Skills { get; set; }

        [StringLength(100)]
        [Display(Name = "المدينة")]
        public string? City { get; set; }

        [StringLength(20)]
        [Display(Name = "رقم الجوال")]
        public string? PhoneNumber { get; set; }

        [StringLength(300)]
        [Display(Name = "رابط LinkedIn")]
        public string? LinkedInUrl { get; set; }

        [StringLength(300)]
        [Display(Name = "رابط GitHub")]
        public string? GitHubUrl { get; set; }

        [StringLength(300)]
        [Display(Name = "اسم ملف السيرة الذاتية")]
        public string? ResumeFileName { get; set; }

        [StringLength(300)]
        [Display(Name = "الاسم الأصلي للملف")]
        public string? ResumeOriginalName { get; set; }

        [Display(Name = "تاريخ رفع السيرة")]
        public DateTime? ResumeUploadedAt { get; set; }

        [Display(Name = "متاح للعمل")]
        public bool IsAvailable { get; set; } = true;

        [StringLength(200)]
        [Display(Name = "العنوان المهني")]
        public string? JobTitle { get; set; }

        [Display(Name = "تاريخ آخر تحديث")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<ProfileView> ProfileViews { get; set; } = new List<ProfileView>();
    }
}

