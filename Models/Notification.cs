using JobSeeker.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobSeeker.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [Required]
        [StringLength(200)]
        [Display(Name = "العنوان")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        [Display(Name = "نص الإشعار")]
        public string Message { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "الرابط")]
        public string? LinkUrl { get; set; }

        [Display(Name = "مقروء")]
        public bool IsRead { get; set; } = false;

        [Display(Name = "نوع الإشعار")]
        public NotificationType Type { get; set; }

        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
