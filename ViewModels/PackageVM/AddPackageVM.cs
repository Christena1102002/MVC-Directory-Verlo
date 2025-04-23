

namespace mvc.ViewModels.PackageVM
{
    public class AddPackageVM
    {
        public int Id { get; set; }   
        public string Name { get; set; } 
        public decimal MonthlyPrice { get; set; } 
        public decimal YearlyPrice { get; set; }
        public string Description { get; set; }

    }
}
