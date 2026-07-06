using Microsoft.AspNetCore.Http;

namespace JobSeeker.Services
{
    public interface IFileService
    {
        Task<string> SaveResumeAsync(IFormFile file);
        void DeleteResume(string? fileName);
        bool IsValidPdf(IFormFile file, out string errorMessage);
        Task<string?> SaveCompanyLogoAsync(IFormFile file);
        void DeleteCompanyLogo(string? fileName);
        bool IsValidImage(IFormFile file, out string errorMessage);
    }
}
