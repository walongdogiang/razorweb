using Org.BouncyCastle.Asn1.Pkcs;
using System.ComponentModel.DataAnnotations;

namespace EFWeb.Validation
{
    public class BirthDateValidation : ValidationAttribute
    {
        public int MinYear { get; set; }
        public int MaxYear { get; set; }

        public BirthDateValidation(int minYear, int maxYear)
        {
            MinYear = minYear;
            MaxYear = maxYear;
        }

        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success; // field này cho phep null

            var val = ((DateTime)value).Year;

            if (val < MinYear || val > MaxYear)
                return new ValidationResult($"{validationContext.DisplayName} phải nằm " +
                    $"trong khoảng từ năm {MinYear} đến {MaxYear}");
            
            return ValidationResult.Success;
        }
    }
}
