using JobSeeker.Constants;
using JobSeeker.Models;
using JobSeeker.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider, bool seedSampleData = true)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Create roles
            string[] roles = { "Admin", "Employer", "JobSeeker" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create default admin
            const string adminEmail = "admin@jobseeker.com";
            const string adminPassword = "Admin@123";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "مدير النظام",
                    UserType = UserType.Admin,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Seed sample data for testing (only if enabled)
            if (seedSampleData)
            {
                await SeedSampleDataAsync(context, userManager);
            }
        }

        private static async Task SeedSampleDataAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            // Only seed if no employers exist
            if (await context.EmployerProfiles.AnyAsync())
                return;

            const string defaultPassword = "Test@123";

            // Sample Employers
            var employers = new List<(ApplicationUser user, EmployerProfile profile)>
            {
                (
                    new ApplicationUser
                    {
                        UserName = "techsolutions@example.com",
                        Email = "techsolutions@example.com",
                        FullName = "أحمد محمد",
                        UserType = UserType.Employer,
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-30)
                    },
                    new EmployerProfile
                    {
                        CompanyName = "حلول التقنية",
                        Industry = "تكنولوجيا المعلومات",
                        CompanySize = "51-200",
                        Website = "https://techsolutions.example.com",
                        Description = "شركة رائدة في مجال تطوير البرمجيات وحلول التقنية للشركات.",
                        City = "الرياض",
                        CreatedAt = DateTime.UtcNow.AddDays(-30),
                        UpdatedAt = DateTime.UtcNow.AddDays(-30)
                    }
                ),
                (
                    new ApplicationUser
                    {
                        UserName = "digitalgrowth@example.com",
                        Email = "digitalgrowth@example.com",
                        FullName = "سارة عبدالله",
                        UserType = UserType.Employer,
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-25)
                    },
                    new EmployerProfile
                    {
                        CompanyName = "النمو الرقمي",
                        Industry = "التسويق الرقمي",
                        CompanySize = "11-50",
                        Website = "https://digitalgrowth.example.com",
                        Description = "وكالة تسويق رقمي متخصصة في حملات التسويق عبر وسائل التواصل الاجتماعي.",
                        City = "جدة",
                        CreatedAt = DateTime.UtcNow.AddDays(-25),
                        UpdatedAt = DateTime.UtcNow.AddDays(-25)
                    }
                ),
                (
                    new ApplicationUser
                    {
                        UserName = "cloudsystems@example.com",
                        Email = "cloudsystems@example.com",
                        FullName = "خالد العلي",
                        UserType = UserType.Employer,
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-20)
                    },
                    new EmployerProfile
                    {
                        CompanyName = "أنظمة السحابة",
                        Industry = "البنية التحتية السحابية",
                        CompanySize = "201-500",
                        Website = "https://cloudsystems.example.com",
                        Description = "نوفر حلول البنية التحتية السحابية والأمن السيبراني للمؤسسات.",
                        City = "الدمام",
                        CreatedAt = DateTime.UtcNow.AddDays(-20),
                        UpdatedAt = DateTime.UtcNow.AddDays(-20)
                    }
                )
            };

            foreach (var (user, profile) in employers)
            {
                var result = await userManager.CreateAsync(user, defaultPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Employer");
                    profile.UserId = user.Id;
                    context.EmployerProfiles.Add(profile);
                }
            }

            await context.SaveChangesAsync();

            // Sample Job Seekers
            var jobSeekers = new List<(ApplicationUser user, JobSeekerProfile profile)>
            {
                (
                    new ApplicationUser
                    {
                        UserName = "omar.dev@example.com",
                        Email = "omar.dev@example.com",
                        FullName = "عمر خالد",
                        PhoneNumber = "0500000001",
                        UserType = UserType.JobSeeker,
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-15)
                    },
                    new JobSeekerProfile
                    {
                        Specialty = "تطوير الويب",
                        JobTitle = "مطور Full Stack",
                        Bio = "مطور ويب شغوف بخبرة 3 سنوات في ASP.NET Core وReact. أبحث عن فرصة في شركة تقنية مبتكرة.",
                        Skills = "C#, ASP.NET Core, React, SQL Server, Entity Framework, JavaScript, HTML, CSS",
                        City = "الرياض",
                        PhoneNumber = "0500000001",
                        LinkedInUrl = "https://linkedin.com/in/omar-dev",
                        GitHubUrl = "https://github.com/omar-dev",
                        IsAvailable = true,
                        UpdatedAt = DateTime.UtcNow.AddDays(-2)
                    }
                ),
                (
                    new ApplicationUser
                    {
                        UserName = "laila.design@example.com",
                        Email = "laila.design@example.com",
                        FullName = "ليلى سعد",
                        PhoneNumber = "0500000002",
                        UserType = UserType.JobSeeker,
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-12)
                    },
                    new JobSeekerProfile
                    {
                        Specialty = "تصميم واجهات المستخدم",
                        JobTitle = "مصممة UI/UX",
                        Bio = "مصممة واجهات مستخدم بخبرة في Figma وAdobe XD. أحب تحويل الأفكار إلى تجارب بصرية رائعة.",
                        Skills = "Figma, Adobe XD, UI Design, UX Research, Prototyping, Wireframing",
                        City = "جدة",
                        PhoneNumber = "0500000002",
                        LinkedInUrl = "https://linkedin.com/in/laila-design",
                        IsAvailable = true,
                        UpdatedAt = DateTime.UtcNow.AddDays(-1)
                    }
                ),
                (
                    new ApplicationUser
                    {
                        UserName = "yasser.data@example.com",
                        Email = "yasser.data@example.com",
                        FullName = "ياسر فهد",
                        PhoneNumber = "0500000003",
                        UserType = UserType.JobSeeker,
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-10)
                    },
                    new JobSeekerProfile
                    {
                        Specialty = "علم البيانات",
                        JobTitle = "محلل بيانات",
                        Bio = "محلل بيانات بخبرة في Python وSQL وPower BI. أعمل على تحويل البيانات إلى رؤى قابلة للتنفيذ.",
                        Skills = "Python, SQL, Power BI, Machine Learning, Pandas, NumPy, Data Visualization",
                        City = "الدمام",
                        PhoneNumber = "0500000003",
                        GitHubUrl = "https://github.com/yasser-data",
                        IsAvailable = true,
                        UpdatedAt = DateTime.UtcNow.AddDays(-3)
                    }
                ),
                (
                    new ApplicationUser
                    {
                        UserName = "nora.mobile@example.com",
                        Email = "nora.mobile@example.com",
                        FullName = "نورة عبدالرحمن",
                        PhoneNumber = "0500000004",
                        UserType = UserType.JobSeeker,
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-8)
                    },
                    new JobSeekerProfile
                    {
                        Specialty = "تطوير تطبيقات الجوال",
                        JobTitle = "مطورة Flutter",
                        Bio = "مطورة تطبيقات جوال بخبرة في Flutter وDart. أنجزت أكثر من 10 تطبيقات على المتاجر.",
                        Skills = "Flutter, Dart, Firebase, REST API, Git, Mobile UI Design",
                        City = "الرياض",
                        PhoneNumber = "0500000004",
                        LinkedInUrl = "https://linkedin.com/in/nora-mobile",
                        GitHubUrl = "https://github.com/nora-mobile",
                        IsAvailable = false,
                        UpdatedAt = DateTime.UtcNow.AddDays(-5)
                    }
                )
            };

            foreach (var (user, profile) in jobSeekers)
            {
                var result = await userManager.CreateAsync(user, defaultPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "JobSeeker");
                    profile.UserId = user.Id;
                    context.JobSeekerProfiles.Add(profile);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
