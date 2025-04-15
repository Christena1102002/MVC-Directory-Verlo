using mvc.ViewModels;
using mvc.Models;
using System.ComponentModel.DataAnnotations;

namespace mvc.Models.CategoryValidation
{
    public class UniqueAttribute : ValidationAttribute
    {

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            var context = (ProjectContext)validationContext.GetService(typeof(ProjectContext));
            if (context == null)
                return new ValidationResult("Database context is not available");

            var categoryName = value.ToString();

            var categoryFromRequest = validationContext.ObjectInstance as CategoryViewModel;
            if (categoryFromRequest == null)
                return new ValidationResult("Invalid Category Object");

            var existingCategory = context.Categories
                .FirstOrDefault(c => c.Name == categoryName && c.Id != categoryFromRequest.Id);

            
            if (existingCategory != null)
            {
                return new ValidationResult("Category name is already taken.");
            }

            return ValidationResult.Success;
        }
    }
}   