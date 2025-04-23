using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using mvc.Enums;
using mvc.Models.Authorize;

namespace mvc.Models
{
    public class Business
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string OwnerId { get; set; }  
        [ForeignKey("OwnerId")]
        public virtual ApplicationUser Owner { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } 

        [Required]
        public int CategoryId { get; set; } 
        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string MainImage { get; set; }

        [Required]
        public string Latitude { get; set; }

        [Required]
        public string Longitude { get; set; }

        [Required]
        public string Address { get; set; }

      
        [Required]
        public BusinessType BusinessType { get; set; } = BusinessType.Regular; 

        public bool IsActive { get; set; } = true; 

        public DateTime? SubscriptionEndDate { get; set; } 
       

        [ForeignKey("PackageId")]
        public int PackageId { get; set; } = 1; 
        public Package Package { get; set; } 

       
        public  ICollection<Review>? Reviews { get; set; }
        public  ICollection<OpeningHour>? OpeningHours { get; set; }
        public  ICollection<Ad>? Advertisements { get; set; }

        public ICollection<BusinessFeatures>? BusinessFeatures { get; set; }
        public ICollection<Checkout>? Checkout { get; set; }

       
        public double GetAverageRating()
        {
            if (Reviews == null || !Reviews.Any())
                return 0;
                
            return Math.Round(Reviews.Average(r => r.Rating), 1);
        }

      
        public Dictionary<int, int> GetRatingDistribution()
        {
            var distribution = new Dictionary<int, int>();
            
          
            for (int i = 1; i <= 5; i++)
            {
                distribution[i] = 0;
            }
            
            if (Reviews == null || !Reviews.Any())
                return distribution;
           
            foreach (var review in Reviews)
            {
                distribution[review.Rating]++;
            }
            
            return distribution;
        }

        public Dictionary<int, int> GetRatingPercentages()
        {
            var distribution = GetRatingDistribution();
            var percentages = new Dictionary<int, int>();
            int total = Reviews?.Count() ?? 0;
            
            if (total == 0)
            {
               
                for (int i = 1; i <= 5; i++)
                {
                    percentages[i] = 0;
                }
            }
            else
            {
             
                for (int i = 1; i <= 5; i++)
                {
                    percentages[i] = (int)Math.Round((double)distribution[i] / total * 100);
                }
            }
            
            return percentages;
        }
    }
}
