using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobSeeker.Models
{
    public class ProfileView
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployerProfileId { get; set; }

        [ForeignKey("EmployerProfileId")]
        public virtual EmployerProfile EmployerProfile { get; set; } = null!;

        [Required]
        public int JobSeekerProfileId { get; set; }

        [ForeignKey("JobSeekerProfileId")]
        public virtual JobSeekerProfile JobSeekerProfile { get; set; } = null!;

        [Display(Name = "تاريخ الزيارة")]
        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "تم إرسال إشعار")]
        public bool NotificationSent { get; set; } = false;
    }
}
