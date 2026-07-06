using JobSeeker.Data;
using JobSeeker.Hubs;
using JobSeeker.Models;
using JobSeeker.Models.Enums;
using JobSeeker.Services;
using JobSeeker.ViewModels;
using JobSeeker.ViewModels.JobSeeker;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.Controllers
{
    [Authorize(Roles = "JobSeeker")]
    public class JobSeekerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileService _fileService;
        private readonly INotificationService _notificationService;
        private readonly IProfileViewService _profileViewService;
        private readonly IHubContext<NotificationHub> _hubContext;

        public JobSeekerController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IFileService fileService,
            INotificationService notificationService,
            IProfileViewService profileViewService,
            IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _fileService = fileService;
            _notificationService = notificationService;
            _profileViewService = profileViewService;
            _hubContext = hubContext;
        }

        private async Task<JobSeekerProfile?> GetCurrentProfileAsync()
        {
            var userId = _userManager.GetUserId(User);
            return await _context.JobSeekerProfiles
                .Include(j => j.User)
                .FirstOrDefaultAsync(j => j.UserId == userId);
        }

        public async Task<IActionResult> Dashboard()
        {
            var profile = await GetCurrentProfileAsync();
            if (profile == null)
                return NotFound();

            var viewsCount = await _profileViewService.GetViewsCountAsync(profile.Id);
            var unreadCount = await _notificationService.GetUnreadCountAsync(profile.UserId);
            var recentViewers = await _profileViewService.GetRecentViewersAsync(profile.Id, 5);
            var recentNotifications = await _notificationService.GetRecentNotificationsAsync(profile.UserId, 5);

            var completionPercentage = CalculateProfileCompletion(profile);
            var tips = GetCompletionTips(profile);

            var model = new JobSeekerDashboardViewModel
            {
                ProfileCompletionPercentage = completionPercentage,
                ProfileViewsCount = viewsCount,
                UnreadNotificationsCount = unreadCount,
                HasResume = !string.IsNullOrEmpty(profile.ResumeFileName),
                ResumeOriginalName = profile.ResumeOriginalName,
                ResumeUploadedAt = profile.ResumeUploadedAt,
                RecentViewers = recentViewers.Select(v => new RecentViewerViewModel
                {
                    CompanyName = v.EmployerProfile.CompanyName,
                    LogoFileName = v.EmployerProfile.LogoFileName,
                    ViewedAt = v.ViewedAt
                }).ToList(),
                RecentNotifications = recentNotifications.Select(n => MapToViewModel(n)).ToList(),
                CompletionTips = tips
            };

            return View(model);
        }

        public async Task<IActionResult> Profile()
        {
            var profile = await GetCurrentProfileAsync();
            if (profile == null)
                return NotFound();

            var viewsCount = await _profileViewService.GetViewsCountAsync(profile.Id);

            var model = new JobSeekerProfileViewModel
            {
                Id = profile.Id,
                FullName = profile.User.FullName,
                Email = profile.User.Email ?? string.Empty,
                Specialty = profile.Specialty ?? string.Empty,
                JobTitle = profile.JobTitle,
                Bio = profile.Bio,
                Skills = profile.Skills,
                City = profile.City,
                PhoneNumber = profile.PhoneNumber,
                LinkedInUrl = profile.LinkedInUrl,
                GitHubUrl = profile.GitHubUrl,
                ResumeFileName = profile.ResumeFileName,
                ResumeOriginalName = profile.ResumeOriginalName,
                ResumeUploadedAt = profile.ResumeUploadedAt,
                IsAvailable = profile.IsAvailable,
                CreatedAt = profile.User.CreatedAt,
                ProfileViewsCount = viewsCount,
                ApplicationsCount = 0
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var profile = await GetCurrentProfileAsync();
            if (profile == null)
                return NotFound();

            var model = new EditProfileViewModel
            {
                Id = profile.Id,
                Specialty = profile.Specialty ?? string.Empty,
                JobTitle = profile.JobTitle,
                Bio = profile.Bio,
                Skills = profile.Skills,
                City = profile.City,
                PhoneNumber = profile.PhoneNumber,
                LinkedInUrl = profile.LinkedInUrl,
                GitHubUrl = profile.GitHubUrl,
                IsAvailable = profile.IsAvailable
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var profile = await GetCurrentProfileAsync();
            if (profile == null)
                return NotFound();

            profile.Specialty = model.Specialty;
            profile.JobTitle = model.JobTitle;
            profile.Bio = model.Bio;
            profile.Skills = model.Skills;
            profile.City = model.City;
            profile.PhoneNumber = model.PhoneNumber;
            profile.LinkedInUrl = model.LinkedInUrl;
            profile.GitHubUrl = model.GitHubUrl;
            profile.IsAvailable = model.IsAvailable;
            profile.UpdatedAt = DateTime.UtcNow;

            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.PhoneNumber = model.PhoneNumber;
                await _userManager.UpdateAsync(user);
            }

            _context.JobSeekerProfiles.Update(profile);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم تحديث الملف الشخصي بنجاح";
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        public async Task<IActionResult> UploadResume()
        {
            var profile = await GetCurrentProfileAsync();
            if (profile == null)
                return NotFound();

            ViewBag.HasResume = !string.IsNullOrEmpty(profile.ResumeFileName);
            ViewBag.ResumeOriginalName = profile.ResumeOriginalName;
            ViewBag.ResumeUploadedAt = profile.ResumeUploadedAt;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadResume(UploadResumeViewModel model)
        {
            var profile = await GetCurrentProfileAsync();
            if (profile == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.HasResume = !string.IsNullOrEmpty(profile.ResumeFileName);
                ViewBag.ResumeOriginalName = profile.ResumeOriginalName;
                ViewBag.ResumeUploadedAt = profile.ResumeUploadedAt;
                return View(model);
            }

            if (!_fileService.IsValidPdf(model.ResumeFile, out var errorMessage))
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                ViewBag.HasResume = !string.IsNullOrEmpty(profile.ResumeFileName);
                ViewBag.ResumeOriginalName = profile.ResumeOriginalName;
                ViewBag.ResumeUploadedAt = profile.ResumeUploadedAt;
                return View(model);
            }

            if (!string.IsNullOrEmpty(profile.ResumeFileName))
            {
                _fileService.DeleteResume(profile.ResumeFileName);
            }

            var newFileName = await _fileService.SaveResumeAsync(model.ResumeFile);

            profile.ResumeFileName = newFileName;
            profile.ResumeOriginalName = model.ResumeFile.FileName;
            profile.ResumeUploadedAt = DateTime.UtcNow;
            profile.UpdatedAt = DateTime.UtcNow;

            _context.JobSeekerProfiles.Update(profile);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم رفع السيرة الذاتية بنجاح";
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteResume()
        {
            var profile = await GetCurrentProfileAsync();
            if (profile == null)
                return NotFound();

            if (!string.IsNullOrEmpty(profile.ResumeFileName))
            {
                _fileService.DeleteResume(profile.ResumeFileName);
                profile.ResumeFileName = null;
                profile.ResumeOriginalName = null;
                profile.ResumeUploadedAt = null;
                profile.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "تم حذف السيرة الذاتية";
            return RedirectToAction(nameof(Profile));
        }

        [AllowAnonymous]
        public async Task<IActionResult> PublicProfile(int id)
        {
            var profile = await _context.JobSeekerProfiles
                .Include(j => j.User)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (profile == null)
                return NotFound();

            if (User.IsInRole("Employer"))
            {
                var employerProfile = await _context.EmployerProfiles
                    .FirstOrDefaultAsync(e => e.UserId == _userManager.GetUserId(User));

                if (employerProfile != null)
                {
                    var view = await _profileViewService.RecordViewAsync(employerProfile.Id, profile.Id);
                    if (view != null)
                    {
                        view.NotificationSent = true;
                        await _context.SaveChangesAsync();

                        var notification = await _notificationService.CreateNotificationAsync(
                            profile.UserId,
                            "زيارة جديدة لملفك الشخصي",
                            $"قامت شركة {employerProfile.CompanyName} بزيارة ملفك الشخصي",
                            $"/JobSeeker/Profile",
                            NotificationType.ProfileView);

                        await _hubContext.Clients.Group(profile.UserId)
                            .SendAsync("ReceiveNotification", new
                            {
                                id = notification.Id,
                                title = notification.Title,
                                message = notification.Message,
                                linkUrl = notification.LinkUrl,
                                type = notification.Type.ToString(),
                                createdAt = notification.CreatedAt,
                                isRead = notification.IsRead
                            });
                    }
                }
            }

            var viewsCount = await _profileViewService.GetViewsCountAsync(profile.Id);

            var model = new JobSeekerProfileViewModel
            {
                Id = profile.Id,
                FullName = profile.User.FullName,
                Email = profile.User.Email ?? string.Empty,
                Specialty = profile.Specialty ?? string.Empty,
                JobTitle = profile.JobTitle,
                Bio = profile.Bio,
                Skills = profile.Skills,
                City = profile.City,
                PhoneNumber = profile.PhoneNumber,
                LinkedInUrl = profile.LinkedInUrl,
                GitHubUrl = profile.GitHubUrl,
                ResumeFileName = profile.ResumeFileName,
                ResumeOriginalName = profile.ResumeOriginalName,
                ResumeUploadedAt = profile.ResumeUploadedAt,
                IsAvailable = profile.IsAvailable,
                CreatedAt = profile.User.CreatedAt,
                ProfileViewsCount = viewsCount,
                ApplicationsCount = 0
            };

            return View(model);
        }

        private static int CalculateProfileCompletion(JobSeekerProfile profile)
        {
            int total = 9;
            int filled = 0;

            if (!string.IsNullOrWhiteSpace(profile.User.FullName)) filled++;
            if (!string.IsNullOrWhiteSpace(profile.Specialty)) filled++;
            if (!string.IsNullOrWhiteSpace(profile.JobTitle)) filled++;
            if (!string.IsNullOrWhiteSpace(profile.Bio)) filled++;
            if (!string.IsNullOrWhiteSpace(profile.Skills)) filled++;
            if (!string.IsNullOrWhiteSpace(profile.City)) filled++;
            if (!string.IsNullOrWhiteSpace(profile.PhoneNumber)) filled++;
            if (!string.IsNullOrWhiteSpace(profile.ResumeFileName)) filled++;
            if (!string.IsNullOrWhiteSpace(profile.LinkedInUrl) || !string.IsNullOrWhiteSpace(profile.GitHubUrl)) filled++;

            return (int)((double)filled / total * 100);
        }

        private static List<string> GetCompletionTips(JobSeekerProfile profile)
        {
            var tips = new List<string>();

            if (string.IsNullOrWhiteSpace(profile.Specialty))
                tips.Add("أضف تخصصك المهني لزيادة فرص ظهورك في البحث");
            if (string.IsNullOrWhiteSpace(profile.JobTitle))
                tips.Add("أضف عنوانك الوظيفي الحالي أو المستهدف");
            if (string.IsNullOrWhiteSpace(profile.Bio))
                tips.Add("اكتب نبذة شخصية تُبرز خبراتك ومهاراتك");
            if (string.IsNullOrWhiteSpace(profile.Skills))
                tips.Add("أضف مهاراتك التقنية والشخصية");
            if (string.IsNullOrWhiteSpace(profile.City))
                tips.Add("حدد مدينتك لمساعدة أصحاب العمل في العثور عليك");
            if (string.IsNullOrWhiteSpace(profile.PhoneNumber))
                tips.Add("أضف رقم الجوال لتسهيل التواصل معك");
            if (string.IsNullOrWhiteSpace(profile.ResumeFileName))
                tips.Add("ارفع سيرتك الذاتية بصيغة PDF");
            if (string.IsNullOrWhiteSpace(profile.LinkedInUrl) && string.IsNullOrWhiteSpace(profile.GitHubUrl))
                tips.Add("أضف روابط LinkedIn أو GitHub لملفك الاحترافي");

            return tips;
        }

        private static NotificationViewModel MapToViewModel(Notification notification)
        {
            return new NotificationViewModel
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                LinkUrl = notification.LinkUrl,
                IsRead = notification.IsRead,
                Type = notification.Type.ToString(),
                CreatedAt = notification.CreatedAt,
                TimeAgo = GetTimeAgo(notification.CreatedAt)
            };
        }

        private static string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;

            if (timeSpan.TotalMinutes < 1)
                return "الآن";
            if (timeSpan.TotalHours < 1)
                return $"{Math.Floor(timeSpan.TotalMinutes)} دقيقة";
            if (timeSpan.TotalDays < 1)
                return $"{Math.Floor(timeSpan.TotalHours)} ساعة";
            if (timeSpan.TotalDays < 30)
                return $"{Math.Floor(timeSpan.TotalDays)} يوم";

            return dateTime.ToString("yyyy-MM-dd");
        }
    }
}
