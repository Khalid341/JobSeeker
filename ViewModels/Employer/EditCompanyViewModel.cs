using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace JobSeeker.ViewModels.Employer
{
    public class EditCompanyViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الشركة مطلوب")]
        [StringLength(200, ErrorMessage = "اسم الشركة يجب أن لا يتجاوز 200 حرف")]
        [Display(Name = "اسم الشركة")]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "المجال يجب أن لا يتجاوز 200 حرف")]
        [Display(Name = "المجال")]
        public string? Industry { get; set; }

        [StringLength(100, ErrorMessage = "حجم الشركة يجب أن لا يتجاوز 100 حرف")]
        [Display(Name = "حجم الشركة")]
        public string? CompanySize { get; set; }

        [StringLength(300, ErrorMessage = "الموقع الإلكتروني يجب أن لا يتجاوز 300 حرف")]
        [Display(Name = "الموقع الإلكتروني")]
        public string? Website { get; set; }

        [StringLength(2000, ErrorMessage = "وصف الشركة يجب أن لا يتجاوز 2000 حرف")]
        [Display(Name = "وصف الشركة")]
        public string? Description { get; set; }

        [StringLength(100, ErrorMessage = "المدينة يجب أن لا تتجاوز 100 حرف")]
        [Display(Name = "المدينة")]
        public string? City { get; set; }

        [Display(Name = "شعار الشركة")]
        public IFormFile? LogoFile { get; set; }

        public string? ExistingLogoFileName { get; set; }
    }
}
