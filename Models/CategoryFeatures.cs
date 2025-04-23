using mvc.Models.CategoryValidation;
using System.ComponentModel.DataAnnotations.Schema;

namespace mvc.Models
{
    public class CategoryFeatures
    {
        public int Id { get; set; }
        
         public string Name { get; set; }
        [ForeignKey("Category")]
        public int CategoryId{ get; set; }
        public Category Category{ get; set; }
    }
}
