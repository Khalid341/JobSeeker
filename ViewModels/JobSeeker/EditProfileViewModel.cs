using JobSeeker.Attributes;
using System.ComponentModel.DataAnnotations;

namespace JobSeeker.ViewModels.JobSeeker
{
    public class EditProfileViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "التخصص مطلوب")]
        [StringLength(200, ErrorMessage = "التخصص يجب أن لا يتجاوز 200 حرف")]
        [Display(Name = "التخصص")]
        [SpecialtyValidation]
        public string Specialty { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "العنوان المهني يجب أن لا يتجاوز 200 حرف")]
        [Display(Name = "العنوان المهني")]
        public string? JobTitle { get; set; }

        [StringLength(2000, ErrorMessage = "النبذة الشخصية يجب أن لا تتجاوز 2000 حرف")]
        [Display(Name = "نبذة شخصية")]
        public string? Bio { get; set; }

        [StringLength(1000, ErrorMessage = "المهارات يجب أن لا تتجاوز 1000 حرف")]
        [Display(Name = "المهارات")]
        public string? Skills { get; set; }

        [StringLength(100, ErrorMessage = "المدينة يجب أن لا تتجاوز 100 حرف")]
        [Display(Name = "المدينة")]
        public string? City { get; set; }

        [StringLength(20, ErrorMessage = "رقم الجوال يجب أن لا يتجاوز 20 رقم")]
        [Display(Name = "رقم الجوال")]
        public string? PhoneNumber { get; set; }

        [StringLength(300, ErrorMessage = "رابط LinkedIn يجب أن لا يتجاوز 300 حرف")]
        [Display(Name = "رابط LinkedIn")]
        public string? LinkedInUrl { get; set; }

        [StringLength(300, ErrorMessage = "رابط GitHub يجب أن لا يتجاوز 300 حرف")]
        [Display(Name = "رابط GitHub")]
        public string? GitHubUrl { get; set; }

        [Display(Name = "متاح للعمل")]
        public bool IsAvailable { get; set; }
    }
}
