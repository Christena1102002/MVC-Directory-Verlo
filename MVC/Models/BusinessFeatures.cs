using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mvc.Models
{
    public class BusinessFeatures
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public int BusinessId { get; set; }
        [ForeignKey("BusinessId")]
        public Business? Business { get; set; }
    }
}
