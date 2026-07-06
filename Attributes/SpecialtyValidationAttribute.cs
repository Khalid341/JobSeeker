using JobSeeker.Constants;
using System.ComponentModel.DataAnnotations;

namespace JobSeeker.Attributes
{
    public class SpecialtyValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success;
            }

            var specialty = value.ToString() ?? string.Empty;
            if (SpecialtiesList.Specialties.Contains(specialty))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("يرجى اختيار تخصص صالح من القائمة");
        }
    }
}
