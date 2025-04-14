using mvc.Attributes;
using mvc.Models.CategoryValidation;

namespace mvc.ViewModels.PackageVM
{
    public class UpdatePackageVM
    {
        public int Id { get; set; }
        
        public string Name { get; set; }
        public decimal MonthlyPrice { get; set; }
        [GreaterThanMonthly]
        public decimal YearlyPrice { get; set; }
    }
}
