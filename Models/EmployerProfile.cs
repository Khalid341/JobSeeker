using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobSeeker.Models
{
    public class EmployerProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [Required]
        [StringLength(200)]
        [Display(Name = "اسم الشركة")]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "المجال")]
        public string? Industry { get; set; }

        [StringLength(100)]
        [Display(Name = "حجم الشركة")]
        public string? CompanySize { get; set; }

        [StringLength(300)]
        [Display(Name = "الموقع الإلكتروني")]
        public string? Website { get; set; }

        [StringLength(2000)]
        [Display(Name = "وصف الشركة")]
        public string? Description { get; set; }

        [StringLength(100)]
        [Display(Name = "المدينة")]
        public string? City { get; set; }

        [StringLength(300)]
        [Display(Name = "شعار الشركة")]
        public string? LogoFileName { get; set; }

        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "تاريخ آخر تحديث")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<ProfileView> ProfileViews { get; set; } = new List<ProfileView>();
    }
}

