using mvc.Enums;
using mvc.Models.Authorize;
using mvc.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace mvc.ViewModels
{
    public class BusinessViewModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Business name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [MinLength(10, ErrorMessage = "Description must be at least 10 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Main image URL is required")]
        public string MainImage { get; set; }

        [Required(ErrorMessage = "Latitude is required")]
        public string Latitude { get; set; } = "0";

        [Required(ErrorMessage = "Longitude is required")]
        public string Longitude { get; set; } = "0";

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }

    
        public List<Category> categories { get; set; } = new List<Category>();

      
        public List<string> businessesNameList { get; set; } = new List<string>(); 

       
        public List<BusinessFeatures> BusinessFeatures { get; set; } = new List<BusinessFeatures>();

        
        public List<OpeningHour> OpeningHours { get; set; } = new List<OpeningHour>();
        
        public bool IsActive { get; set; } = false;
        
        public DateTime? SubscriptionEndDate { get; set; }

       
        public int PackageId { get; set; } = 1; 
        public BusinessType? BusinessType { get; set; } = Enums.BusinessType.Regular;

       
        public List<Package>? Packages { get; set; }
    }
}
