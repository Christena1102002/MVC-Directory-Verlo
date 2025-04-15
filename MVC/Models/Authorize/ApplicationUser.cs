using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace mvc.Models.Authorize
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string Address { get; set; }
        
        public DateTime RegisterDate { get; set; } = DateTime.UtcNow;
    }
}
