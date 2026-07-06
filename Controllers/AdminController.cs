using JobSeeker.Data;
using JobSeeker.Models;
using JobSeeker.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var totalUsers = await _userManager.Users.CountAsync();
            var totalEmployers = await _context.EmployerProfiles.CountAsync();
            var totalJobSeekers = await _context.JobSeekerProfiles.CountAsync();
            var totalResumes = await _context.JobSeekerProfiles
                .CountAsync(j => !string.IsNullOrEmpty(j.ResumeFileName));
            var totalProfileViews = await _context.ProfileViews.CountAsync();

            var recentUsers = await _userManager.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(10)
                .Select(u => new RecentUserViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email ?? string.Empty,
                    UserType = u.UserType.ToString(),
                    CreatedAt = u.CreatedAt,
                    IsActive = u.IsActive
                })
                .ToListAsync();

            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.UtcNow.Date.AddDays(-i))
                .Reverse()
                .ToList();

            var viewsChartData = new List<DailyViewChartViewModel>();
            foreach (var date in last7Days)
            {
                var count = await _context.ProfileViews
                    .CountAsync(p => p.ViewedAt.Date == date.Date);

                viewsChartData.Add(new DailyViewChartViewModel
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    Count = count
                });
            }

            var model = new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalEmployers = totalEmployers,
                TotalJobSeekers = totalJobSeekers,
                TotalResumes = totalResumes,
                TotalProfileViews = totalProfileViews,
                RecentUsers = recentUsers,
                ViewsChartData = viewsChartData
            };

            return View(model);
        }

        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            var model = new List<UserListViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Add(new UserListViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    UserType = user.UserType.ToString(),
                    PhoneNumber = user.PhoneNumber,
                    CreatedAt = user.CreatedAt,
                    IsActive = user.IsActive,
                    EmailConfirmed = user.EmailConfirmed,
                    Roles = roles
                });
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            var status = user.IsActive ? "تفعيل" : "تعطيل";
            TempData["Success"] = $"تم {status} الحساب بنجاح";
            return RedirectToAction(nameof(Users));
        }

        public async Task<IActionResult> Employers()
        {
            var employers = await _context.EmployerProfiles
                .Include(e => e.User)
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => new EmployerListViewModel
                {
                    Id = e.Id,
                    UserId = e.UserId,
                    FullName = e.User.FullName,
                    Email = e.User.Email ?? string.Empty,
                    CompanyName = e.CompanyName,
                    Industry = e.Industry,
                    City = e.City,
                    CreatedAt = e.CreatedAt,
                    IsActive = e.User.IsActive,
                    JobPostsCount = 0
                })
                .ToListAsync();

            return View(employers);
        }

        public async Task<IActionResult> JobSeekers()
        {
            var jobSeekers = await _context.JobSeekerProfiles
                .Include(j => j.User)
                .OrderByDescending(j => j.User.CreatedAt)
                .Select(j => new JobSeekerListViewModel
                {
                    Id = j.Id,
                    UserId = j.UserId,
                    FullName = j.User.FullName,
                    Email = j.User.Email ?? string.Empty,
                    Specialty = j.Specialty,
                    JobTitle = j.JobTitle,
                    City = j.City,
                    IsAvailable = j.IsAvailable,
                    HasResume = !string.IsNullOrEmpty(j.ResumeFileName),
                    CreatedAt = j.User.CreatedAt,
                    IsActive = j.User.IsActive,
                    ViewsCount = _context.ProfileViews.Count(p => p.JobSeekerProfileId == j.Id)
                })
                .ToListAsync();

            return View(jobSeekers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var currentAdminId = _userManager.GetUserId(User);
            if (user.Id == currentAdminId)
            {
                TempData["Error"] = "لا يمكنك حذف حسابك الحالي";
                return RedirectToAction(nameof(Users));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "تم حذف المستخدم بنجاح";
            }
            else
            {
                TempData["Error"] = "حدث خطأ أثناء حذف المستخدم";
            }

            return RedirectToAction(nameof(Users));
        }
    }
}
