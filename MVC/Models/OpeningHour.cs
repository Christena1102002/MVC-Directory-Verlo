using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace mvc.Models
{
    public class OpeningHour
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BusinessId { get; set; }

        [Required]
        [Range(1, 7, ErrorMessage = "Day of week must be between 1 and 7")]
        public int DayOfWeek { get; set; }
        public TimeSpan? OpenTime { get; set; }

        public TimeSpan? CloseTime { get; set; }

        public bool IsClosed { get; set; }

        public string? CloseReason { get; set; }

        // Navigation Properties
        [ForeignKey("BusinessId")]
        public Business? Business { get; set; }

    }
}