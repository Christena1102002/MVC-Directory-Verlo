using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using mvc.Models.Authorize;
using mvc.Enums;

namespace mvc.Models
{
    public class Checkout
    {
        [Key]
        public int Id { get; set; }

        
        public string UserId { get; set; }

        
        public decimal Amount { get; set; }

        
        public PaymentMethod PaymentMethod { get; set; }

       
        public PaymentStatus PaymentStatus { get; set; }
        public SubscriptionType SubscriptionType { get; set; }
     
        public string? TransactionId { get; set; }

        public DateTime CreatedAt { get; set; }
        [ForeignKey("Business")]
        public int BusinessId { get; set; }
        [ForeignKey("Package")]
        public int PackageId { get; set; }
        public Business? Business { get; set; }
        public Package? Package { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        public Checkout()
        {
            CreatedAt = DateTime.UtcNow;
            PaymentStatus = PaymentStatus.Pending;
        }
    }
   
}
