using Microsoft.AspNetCore.Http;

namespace JobSeeker.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private const long MaxResumeSize = 5 * 1024 * 1024; // 5MB
        private const long MaxLogoSize = 2 * 1024 * 1024; // 2MB

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public bool IsValidPdf(IFormFile file, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (file == null || file.Length == 0)
            {
                errorMessage = "يرجى اختيار ملف";
                return false;
            }

            if (file.Length > MaxResumeSize)
            {
                errorMessage = "حجم الملف يجب أن لا يتجاوز 5 ميجابايت";
                return false;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".pdf")
            {
                errorMessage = "يسمح فقط بملفات PDF";
                return false;
            }

            // Validate content type
            var allowedTypes = new[] { "application/pdf", "application/x-pdf" };
            if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()) && file.ContentType != "application/octet-stream")
            {
                errorMessage = "نوع الملف غير صالح. يسمح فقط بملفات PDF";
                return false;
            }

            // Validate file signature (magic bytes)
            using var stream = file.OpenReadStream();
            var buffer = new byte[4];
            stream.ReadExactly(buffer, 0, 4);
            var pdfSignature = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF
            if (!buffer.SequenceEqual(pdfSignature))
            {
                errorMessage = "ملف PDF غير صالح";
                return false;
            }

            return true;
        }

        public async Task<string> SaveResumeAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "resumes");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = $"{Guid.NewGuid()}.pdf";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        public void DeleteResume(string? fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;

            var filePath = Path.Combine(_environment.WebRootPath, "uploads", "resumes", fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public bool IsValidImage(IFormFile file, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (file == null || file.Length == 0)
            {
                errorMessage = "يرجى اختيار صورة";
                return false;
            }

            if (file.Length > MaxLogoSize)
            {
                errorMessage = "حجم الصورة يجب أن لا يتجاوز 2 ميجابايت";
                return false;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            if (!allowedExtensions.Contains(extension))
            {
                errorMessage = "يسمح فقط بصور بصيغة JPG, PNG, GIF";
                return false;
            }

            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
            if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                errorMessage = "نوع الصورة غير صالح";
                return false;
            }

            return true;
        }

        public async Task<string?> SaveCompanyLogoAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "logos");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        public void DeleteCompanyLogo(string? fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;

            var filePath = Path.Combine(_environment.WebRootPath, "uploads", "logos", fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
