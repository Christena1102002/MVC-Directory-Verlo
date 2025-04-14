using mvc.Models.Authorize;
using mvc.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mvc.Models
{
    public class Chat
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SenderId { get; set; }

        //[Required]
        //public string ReceiverId { get; set; }

        [Required]
        public string Message { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }

        [Required]
        public string ConversationId { get; set; }

        // Optional reference to a business if the chat is about a specific business
        public int? BusinessId { get; set; }

        // Navigation Properties
        [ForeignKey("SenderId")]
        public virtual ApplicationUser Sender { get; set; }

        //[ForeignKey("ReceiverId")]
        //public virtual ApplicationUser Receiver { get; set; }

        [ForeignKey("BusinessId")]
        public virtual Business Business { get; set; }

        public Chat()
        {
            CreatedAt = DateTime.UtcNow;
            IsRead = false;
        }
    }
}
