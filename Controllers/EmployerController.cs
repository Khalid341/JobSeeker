using JobSeeker.Data;
using JobSeeker.Hubs;
using JobSeeker.Models;
using JobSeeker.Models.Enums;
using JobSeeker.Services;
using JobSeeker.ViewModels.Employer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.Controllers
{
    [Authorize(Roles = "Employer")]
    public class EmployerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileService _fileService;
        private readonly INotificationService _notificationService;
        private readonly IProfileViewService _profileViewService;
        private readonly IHubContext<NotificationHub> _hubContext;

        public EmployerController(
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

        private async Task<EmployerProfile?> GetCurrentProfileAsync()
        {
            var userId = _userManager.GetUserId(User);
            return await _context.EmployerProfiles
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.UserId == userId);
        }

        public async Task<IActionResult> Dashboard()
        {
            var profile = await GetCurrentProfileAsync();
            if (profile == null)
                return NotFound();

            var totalCandidates = await _context.JobSeekerProfiles.CountAsync();
            var profilesViewed = await _context.ProfileViews
                .CountAsync(p => p.EmployerProfileId == profile.Id);
            var availableCandidates = await _context.JobSeekerProfiles
                .CountAsync(j => j.IsAvailable);

            var recentCandidates = await _context.JobSeekerProfiles
                .Where(j => j.IsAvailable)
                .OrderByDescending(j => j.UpdatedAt)
                .Take(6)
                .Select(j => new CandidateResultViewModel
                {
                    Id = j.Id,
                    FullName = j.User.FullName,
                    Specialty = j.Specialty,
                    JobTitle = j.JobTitle,
                    City = j.City,
                    Skills = j.Skills,
                    IsAvailable = j.IsAvailable,
                    ViewsCount = _context.ProfileViews.Count(p => p.JobSeekerProfileId == j.Id),
                    UpdatedAt = j.UpdatedAt
                })
                .ToListAsync();

            var model = new EmployerDashboardViewModel
            {
                TotalCandidates = totalCandidates,
                ProfilesViewedCount = profilesViewed,
                AvailableCandidatesCount = availableCandidates,
                RecentCandidates = recentCandidates
            };

            return View(model);
        }

        public async Task<IActionResult> Profile()
        {
            var profile = await GetCurrentProfileAsync();
            if (profile == null)
                return NotFound();

            var viewsCount = await _context.ProfileViews
                .CountAsync(p => p.EmployerProfileId == profile.Id);

            var model = new EmployerProfileViewModel
            {
                Id = profile.Id,
                FullName = profile.User.FullName,
                Email = profile.User.Email ?? string.Empty,
                CompanyName = profile.CompanyName,
                Industry = profile.Industry,
                CompanySize = profile.CompanySize,
                Website = profile.Website,
                Description = profile.Description,
                City = profile.City,
                LogoFileName = profile.LogoFileName,
                CreatedAt = profile.CreatedAt,
                JobPostsCount = 0,
                ProfileViewsCount = viewsCount
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditCompany()
        {
            var profile = await GetCurrentProfileAsync();
            if (profile == null)
                return NotFound();

            var model = new EditCompanyViewModel
            {
                Id = profile.Id,
                CompanyName = profile.CompanyName,
                Industry = profile.Industry,
                CompanySize = profile.CompanySize,
                Website = profile.Website,
                Description = profile.Description,
                City = profile.City,
                ExistingLogoFileName = profile.LogoFileName
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCompany(EditCompanyViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var profile = await GetCurrentProfileAsync();
            if (profile == null)
                return NotFound();

            profile.CompanyName = model.CompanyName;
            profile.Industry = model.Industry;
            profile.CompanySize = model.CompanySize;
            profile.Website = model.Website;
            profile.Description = model.Description;
            profile.City = model.City;
            profile.UpdatedAt = DateTime.UtcNow;

            if (model.LogoFile != null)
            {
                if (!_fileService.IsValidImage(model.LogoFile, out var errorMessage))
                {
                    ModelState.AddModelError(string.Empty, errorMessage);
                    model.ExistingLogoFileName = profile.LogoFileName;
                    return View(model);
                }

                if (!string.IsNullOrEmpty(profile.LogoFileName))
                {
                    _fileService.DeleteCompanyLogo(profile.LogoFileName);
                }

                profile.LogoFileName = await _fileService.SaveCompanyLogoAsync(model.LogoFile);
            }

            _context.EmployerProfiles.Update(profile);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم تحديث بيانات الشركة بنجاح";
            return RedirectToAction(nameof(Profile));
        }

        public async Task<IActionResult> SearchCandidates(CandidateSearchViewModel? model = null)
        {
            model ??= new CandidateSearchViewModel();

            var query = _context.JobSeekerProfiles
                .Include(j => j.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(model.Specialty))
                query = query.Where(j => j.Specialty != null && j.Specialty.Contains(model.Specialty));

            if (!string.IsNullOrWhiteSpace(model.City))
                query = query.Where(j => j.City != null && j.City.Contains(model.City));

            if (!string.IsNullOrWhiteSpace(model.Skills))
                query = query.Where(j => j.Skills != null && j.Skills.Contains(model.Skills));

            if (model.IsAvailableOnly)
                query = query.Where(j => j.IsAvailable);

            query = model.SortOrder switch
            {
                CandidateSortOrder.MostViewed => query.OrderByDescending(j => _context.ProfileViews.Count(p => p.JobSeekerProfileId == j.Id)),
                _ => query.OrderByDescending(j => j.UpdatedAt)
            };

            var candidates = await query
                .Select(j => new CandidateResultViewModel
                {
                    Id = j.Id,
                    FullName = j.User.FullName,
                    Specialty = j.Specialty,
                    JobTitle = j.JobTitle,
                    City = j.City,
                    Skills = j.Skills,
                    IsAvailable = j.IsAvailable,
                    ViewsCount = _context.ProfileViews.Count(p => p.JobSeekerProfileId == j.Id),
                    UpdatedAt = j.UpdatedAt
                })
                .ToListAsync();

            ViewBag.Candidates = candidates;
            return View(model);
        }

        public async Task<IActionResult> CandidateDetails(int id)
        {
            var profile = await GetCurrentProfileAsync();
            if (profile == null)
                return NotFound();

            var candidate = await _context.JobSeekerProfiles
                .Include(j => j.User)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (candidate == null)
                return NotFound();

            var alreadyViewedToday = await _profileViewService.HasViewedInLast24HoursAsync(profile.Id, candidate.Id);
            var view = await _profileViewService.RecordViewAsync(profile.Id, candidate.Id);

            if (view != null)
            {
                view.NotificationSent = true;
                await _context.SaveChangesAsync();

                var notification = await _notificationService.CreateNotificationAsync(
                    candidate.UserId,
                    "زيارة جديدة لملفك الشخصي",
                    $"قامت شركة {profile.CompanyName} بزيارة ملفك الشخصي",
                    $"/JobSeeker/Profile",
                    NotificationType.ProfileView);

                await _hubContext.Clients.Group(candidate.UserId)
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

            var viewsCount = await _profileViewService.GetViewsCountAsync(candidate.Id);

            var model = new CandidateDetailsViewModel
            {
                Id = candidate.Id,
                FullName = candidate.User.FullName,
                Email = candidate.User.Email ?? string.Empty,
                Specialty = candidate.Specialty,
                JobTitle = candidate.JobTitle,
                Bio = candidate.Bio,
                Skills = candidate.Skills,
                City = candidate.City,
                PhoneNumber = candidate.PhoneNumber,
                LinkedInUrl = candidate.LinkedInUrl,
                GitHubUrl = candidate.GitHubUrl,
                IsAvailable = candidate.IsAvailable,
                ResumeFileName = candidate.ResumeFileName,
                ResumeOriginalName = candidate.ResumeOriginalName,
                ResumeUploadedAt = candidate.ResumeUploadedAt,
                HasResume = !string.IsNullOrEmpty(candidate.ResumeFileName),
                ViewsCount = viewsCount,
                AlreadyViewedToday = alreadyViewedToday
            };

            return View(model);
        }

        public async Task<IActionResult> DownloadResume(int id)
        {
            var profile = await GetCurrentProfileAsync();
            if (profile == null)
                return NotFound();

            var candidate = await _context.JobSeekerProfiles
                .FirstOrDefaultAsync(j => j.Id == id);

            if (candidate == null || string.IsNullOrEmpty(candidate.ResumeFileName))
                return NotFound();

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "resumes", candidate.ResumeFileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var mimeType = "application/pdf";
            var fileName = candidate.ResumeOriginalName ?? $"resume-{id}.pdf";

            return PhysicalFile(filePath, mimeType, fileName);
        }
    }
}
