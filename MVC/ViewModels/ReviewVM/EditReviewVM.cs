using System.ComponentModel.DataAnnotations;

namespace mvc.ViewModels.ReviewVM
{
    public class EditReviewVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; }

        [Required, Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Comment is required")]
        [StringLength(500, ErrorMessage = "Comment cannot be longer than 500 characters")]
        public string Comment { get; set; }
    }
}
