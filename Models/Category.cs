using mvc.Models.CategoryValidation;
using System.ComponentModel.DataAnnotations;

namespace mvc.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Unique]
        public string Name { get; set; }

      
        public string? Icon { get; set; }
       
        public List<CategoryFeatures>? CategoryFeatures { get; set; } 

       

        public ICollection<Business>? Businesses { get; set; }

      
    }
}