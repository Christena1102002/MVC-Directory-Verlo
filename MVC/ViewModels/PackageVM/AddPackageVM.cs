using mvc.Attributes;

namespace mvc.ViewModels.PackageVM
{
    public class AddPackageVM
    {
        public string Name { get; set; }
        public decimal MonthlyPrice { get; set; }
        [GreaterThanMonthly]
        public decimal YearlyPrice { get; set; }
    }
}
