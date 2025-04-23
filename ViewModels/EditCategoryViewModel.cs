namespace mvc.ViewModels
{
    public class EditCategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Icone { get; set; }
        public List<EditFeatureViewModel> Features { get; set; } = new List<EditFeatureViewModel>();
    }

    public class EditFeatureViewModel
    {
        public int FeatureID { get; set; }  
        public string NameFeature { get; set; }
    }
}