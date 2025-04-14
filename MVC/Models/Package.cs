using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mvc.Models
{
    public class Package
    {
        [Key]
        public int Id { get; set; }   

        [Required]
        public string Name { get; set; } // sponsared , Featured , Basic 

        [Required]
        public decimal MonthlyPrice { get; set; } 

        [Required]
        public decimal YearlyPrice { get; set; }
        public ICollection<Business>? Businesses { get; set; } 

       
        
    }
}
