using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mvc.Models
{
    public class PackageFeature
    {
        public int Id { get; set; }
        
        [Required]
        public int PackageId { get; set; }
        
        [Required]
        [StringLength(255)]
        public string Description { get; set; }
        
        [Required]
        public bool IsIncluded { get; set; }
        
       
        [ForeignKey("PackageId")]
        public virtual Package Package { get; set; }
    }
}
