
using mvc.Models;
using mvc.Models.CategoryValidation;
using System.ComponentModel.DataAnnotations;

namespace mvc.ViewModels
{
    public class CategoryViewModel
    {
        public int Id { get; set; }

        [Unique] 
        public string Name { get; set; }
       
        public string? Icon { get; set; }

        public List<FeatureViewModel> Features { get; set; } = new List<FeatureViewModel>();
    }
    public class FeatureViewModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }
}














