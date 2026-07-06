using JobSeeker.Attributes;
using System.ComponentModel.DataAnnotations;

namespace JobSeeker.ViewModels.Account
{
    public class RegisterJobSeekerViewModel
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [StringLength(100, ErrorMessage = "الاسم الكامل يجب أن لا يتجاوز 100 حرف")]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صالح")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [StringLength(100, ErrorMessage = "كلمة المرور يجب أن تكون على الأقل {2} حرف", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "تأكيد كلمة المرور")]
        [Compare("Password", ErrorMessage = "كلمة المرور وتأكيدها غير متطابقين")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "التخصص مطلوب")]
        [StringLength(200, ErrorMessage = "التخصص يجب أن لا يتجاوز 200 حرف")]
        [Display(Name = "التخصص")]
        [SpecialtyValidation]
        public string Specialty { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "العنوان المهني يجب أن لا يتجاوز 200 حرف")]
        [Display(Name = "العنوان المهني")]
        public string? JobTitle { get; set; }

        [StringLength(100, ErrorMessage = "المدينة يجب أن لا تتجاوز 100 حرف")]
        [Display(Name = "المدينة")]
        public string? City { get; set; }

        [StringLength(20, ErrorMessage = "رقم الجوال يجب أن لا يتجاوز 20 رقم")]
        [Display(Name = "رقم الجوال")]
        public string? PhoneNumber { get; set; }
    }
}
