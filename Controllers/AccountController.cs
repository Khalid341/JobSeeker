using JobSeeker.Data;
using JobSeeker.Models;
using JobSeeker.Models.Enums;
using JobSeeker.Services;
using JobSeeker.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        private bool RequireEmailConfirmation => _configuration.GetValue<bool>("EmailSettings:RequireEmailConfirmation");

        private async Task SendEmailConfirmationAsync(ApplicationUser user)
        {
            if (!_emailService.IsConfigured())
            {
                _logger.LogWarning("Email service not configured. Skipping confirmation email for {Email}", user.Email);
                return;
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action(
                "ConfirmEmail",
                "Account",
                new { userId = user.Id, token = token },
                protocol: Request.Scheme,
                host: Request.Host.ToString());

            var subject = "تأكيد بريدك الإلكتروني - JobSeeker";
            var body = $@"
                <div style='direction: rtl; text-align: right; font-family: Arial, sans-serif;'>
                    <h2>مرحباً {user.FullName}،</h2>
                    <p>شكراً لتسجيلك في JobSeeker. يرجى تأكيد بريدك الإلكتروني بالضغط على الرابط أدناه:</p>
                    <p><a href='{callbackUrl}' style='padding: 10px 20px; background-color: #0d6efd; color: white; text-decoration: none; border-radius: 5px;'>تأكيد البريد الإلكتروني</a></p>
                    <p>أو انسخ هذا الرابط ولصقه في المتصفح:</p>
                    <p>{callbackUrl}</p>
                    <hr>
                    <p style='color: #6c757d;'>إذا لم تقم بتسجيل حساب، يرجى تجاهل هذه الرسالة.</p>
                </div>";

            await _emailService.SendEmailAsync(user.Email!, subject, body);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "البريد الإلكتروني أو كلمة المرور غير صحيحة");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "تم تعطيل حسابك. يرجى التواصل مع الإدارة");
                return View(model);
            }

            if (RequireEmailConfirmation && !await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError(string.Empty, "يرجى تأكيد بريدك الإلكتروني قبل تسجيل الدخول. تحقق من صندوق الوارد.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                if (roles.Contains("Admin"))
                    return RedirectToAction("Dashboard", "Admin");
                if (roles.Contains("Employer"))
                    return RedirectToAction("Dashboard", "Employer");
                if (roles.Contains("JobSeeker"))
                    return RedirectToAction("Dashboard", "JobSeeker");

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "البريد الإلكتروني أو كلمة المرور غير صحيحة");
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult RegisterJobSeeker()
        {
            return View(new RegisterJobSeekerViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("register")]
        public async Task<IActionResult> RegisterJobSeeker(RegisterJobSeekerViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                UserType = UserType.JobSeeker,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "JobSeeker");

                var profile = new JobSeekerProfile
                {
                    UserId = user.Id,
                    Specialty = model.Specialty,
                    JobTitle = model.JobTitle,
                    City = model.City,
                    PhoneNumber = model.PhoneNumber,
                    IsAvailable = true,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.JobSeekerProfiles.Add(profile);
                await _context.SaveChangesAsync();

                await SendEmailConfirmationAsync(user);

                if (RequireEmailConfirmation)
                {
                    TempData["Success"] = "تم إنشاء حسابك بنجاح! يرجى التحقق من بريدك الإلكتروني لتفعيل الحساب.";
                    return RedirectToAction("Login");
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["Success"] = "تم إنشاء حسابك بنجاح!";
                return RedirectToAction("Dashboard", "JobSeeker");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult RegisterEmployer()
        {
            return View(new RegisterEmployerViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("register")]
        public async Task<IActionResult> RegisterEmployer(RegisterEmployerViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                UserType = UserType.Employer,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Employer");

                var profile = new EmployerProfile
                {
                    UserId = user.Id,
                    CompanyName = model.CompanyName,
                    Industry = model.Industry,
                    City = model.City,
                    Website = model.Website,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.EmployerProfiles.Add(profile);
                await _context.SaveChangesAsync();

                await SendEmailConfirmationAsync(user);

                if (RequireEmailConfirmation)
                {
                    TempData["Success"] = "تم إنشاء حساب الشركة بنجاح! يرجى التحقق من بريدك الإلكتروني لتفعيل الحساب.";
                    return RedirectToAction("Login");
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["Success"] = "تم إنشاء حساب الشركة بنجاح!";
                return RedirectToAction("Dashboard", "Employer");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
                return RedirectToAction("Index", "Home");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return RedirectToAction("Index", "Home");

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                TempData["Success"] = "تم تأكيد بريدك الإلكتروني بنجاح! يمكنك الآن تسجيل الدخول.";
            }
            else
            {
                TempData["Error"] = "رابط التأكيد غير صالح أو منتهي الصلاحية.";
            }

            return RedirectToAction("Login");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["Success"] = "تم تغيير كلمة المرور بنجاح";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult RegisterAdmin()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterAdmin(RegisterAdminViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                UserType = UserType.Admin,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                await SendEmailConfirmationAsync(user);

                TempData["Success"] = RequireEmailConfirmation
                    ? "تم إنشاء حساب المدير بنجاح. يرجى مطالبة المدير بالتحقق من بريده الإلكتروني."
                    : "تم إنشاء حساب المدير بنجاح";
                return RedirectToAction("Users", "Admin");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
    }
}
