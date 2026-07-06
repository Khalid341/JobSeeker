using JobSeeker.Attributes;
using System.ComponentModel.DataAnnotations;

namespace JobSeeker.ViewModels.Employer
{
    public class CandidateSearchViewModel
    {
        [StringLength(200)]
        [Display(Name = "التخصص")]
        [SpecialtyValidation]
        public string? Specialty { get; set; }

        [StringLength(100)]
        [Display(Name = "المدينة")]
        public string? City { get; set; }

        [StringLength(500)]
        [Display(Name = "المهارات")]
        public string? Skills { get; set; }

        [Display(Name = "متاح للعمل فقط")]
        public bool IsAvailableOnly { get; set; }

        [Display(Name = "الترتيب")]
        public CandidateSortOrder SortOrder { get; set; } = CandidateSortOrder.Newest;
    }

    public enum CandidateSortOrder
    {
        Newest,
        MostViewed
    }
}
