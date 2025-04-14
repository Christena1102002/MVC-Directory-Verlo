using mvc.ViewModels.PackageVM;
using System.ComponentModel.DataAnnotations;

namespace mvc.Attributes
{
    public class GreaterThanMonthlyAttribute: ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var propert = validationContext.ObjectType.GetProperty("MonthlyPrice");
            decimal monthlyPrice= (decimal)propert.GetValue(validationContext.ObjectInstance);
            if (value is decimal yearlyPrice && yearlyPrice<=monthlyPrice)
            {
                return new ValidationResult("Yearly price must be greater than monthly price.");
            }
            return ValidationResult.Success;
        }
    }
  
}
