using System.ComponentModel.DataAnnotations;

namespace JobSeeker.ViewModels.Account
{
    public class RegisterEmployerViewModel
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [StringLength(100, ErrorMessage = "الاسم الكامل يجب أن لا يتجاوز 100 حرف")]
        [Display(Name = "اسم ممثل الشركة")]
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

        [Required(ErrorMessage = "اسم الشركة مطلوب")]
        [StringLength(200, ErrorMessage = "اسم الشركة يجب أن لا يتجاوز 200 حرف")]
        [Display(Name = "اسم الشركة")]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "المجال يجب أن لا يتجاوز 200 حرف")]
        [Display(Name = "المجال")]
        public string? Industry { get; set; }

        [StringLength(100, ErrorMessage = "المدينة يجب أن لا تتجاوز 100 حرف")]
        [Display(Name = "المدينة")]
        public string? City { get; set; }

        [StringLength(300, ErrorMessage = "الموقع الإلكتروني يجب أن لا يتجاوز 300 حرف")]
        [Display(Name = "الموقع الإلكتروني")]
        public string? Website { get; set; }
    }
}
