using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace JobSeeker.ViewModels.JobSeeker
{
    public class UploadResumeViewModel
    {
        [Required(ErrorMessage = "يرجى اختيار ملف السيرة الذاتية")]
        [Display(Name = "ملف السيرة الذاتية")]
        public IFormFile ResumeFile { get; set; } = null!;
    }
}
