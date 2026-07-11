using JobSeeker.Data;
using JobSeeker.Hubs;
using JobSeeker.Middleware;
using JobSeeker.Models;
using JobSeeker.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

// Configure Serilog early to capture startup errors
var logsPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "Logs");
Directory.CreateDirectory(logsPath);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine(logsPath, "log-.txt"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting LinkMe web application");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity with roles - production hardened
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Strong password policy
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 4;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false; // Set to true after configuring SMTP
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Authentication cookie settings - production hardened
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    // Use SameAsRequest until HTTPS/SSL is enabled, then change to Always
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// SignalR for real-time notifications
builder.Services.AddSignalR();

// Email service
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

// Data Protection - persist keys to disk for shared hosting
var dataProtectionPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtection-Keys");
Directory.CreateDirectory(dataProtectionPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
    .SetApplicationName("JobSeeker");

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("register", opt =>
    {
        opt.PermitLimit = 3;
        opt.Window = TimeSpan.FromMinutes(5);
        opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
        await context.HttpContext.Response.WriteAsync("عدد الطلبات كثير جداً. يرجى المحاولة لاحقاً.", token);
    };
});

// Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// Response Caching
builder.Services.AddResponseCaching();

// Custom services
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IProfileViewService, ProfileViewService>();

var app = builder.Build();

// Seed roles and admin
using (var scope = app.Services.CreateScope())
{
    var seedSampleData = app.Configuration.GetValue<bool>("SeedSampleData", app.Environment.IsDevelopment());
    await DbInitializer.SeedAsync(scope.ServiceProvider, seedSampleData);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseResponseCompression();
app.UseResponseCaching();

// Static files with caching headers for production
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        if (!app.Environment.IsDevelopment())
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=604800");
        }
    }
});

app.UseRouting();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.UseStatusCodePagesWithReExecute("/Home/NotFoundPage", "?statusCode={0}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<NotificationHub>("/notificationHub");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
