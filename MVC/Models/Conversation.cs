using mvc.Models.Authorize;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mvc.Models
{
    public class Conversation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public string? AdminId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastMessageAt { get; set; }
        public bool IsClosed { get; set; } = false;

        public bool IsAdminBroadcast { get; set; } = false;

        public bool IsReadByAdmin { get; set; } = false;
        public bool IsReadByUser { get; set; } = false;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("AdminId")]
        public virtual ApplicationUser? Admin { get; set; }

        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}