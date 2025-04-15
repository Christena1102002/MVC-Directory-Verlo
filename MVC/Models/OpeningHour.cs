using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace mvc.Models
{
    public class OpeningHour
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Business")]
        public int BusinessId { get; set; }

       
        public int DayOfWeek { get; set; }
        public TimeSpan? OpenTime { get; set; }

        public TimeSpan? CloseTime { get; set; }

        public bool IsClosed { get; set; }

        public string? CloseReason { get; set; }

       
        [ForeignKey("BusinessId")]
        public Business? Business { get; set; }

    }
}