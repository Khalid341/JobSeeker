using JobSeeker.Data;
using JobSeeker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace JobSeeker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var stats = new
            {
                EmployersCount = await _context.EmployerProfiles.CountAsync(),
                JobSeekersCount = await _context.JobSeekerProfiles.CountAsync(),
                ResumesCount = await _context.JobSeekerProfiles.CountAsync(j => !string.IsNullOrEmpty(j.ResumeFileName)),
                ProfileViewsCount = await _context.ProfileViews.CountAsync()
            };

            ViewBag.Stats = stats;
            return View();
        }

        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any, NoStore = false)]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("/Home/NotFound")]
        public IActionResult NotFoundPage()
        {
            Response.StatusCode = 404;
            return View("NotFound");
        }
    }
}
