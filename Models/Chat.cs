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

        [Required]
        public string Message { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }

        [Required]
        public string ConversationId { get; set; }

        public int? BusinessId { get; set; }

        [ForeignKey("SenderId")]
        public virtual ApplicationUser Sender { get; set; }

       

        [ForeignKey("BusinessId")]
        public virtual Business Business { get; set; }

        public Chat()
        {
            CreatedAt = DateTime.UtcNow;
            IsRead = false;
        }
    }
}
