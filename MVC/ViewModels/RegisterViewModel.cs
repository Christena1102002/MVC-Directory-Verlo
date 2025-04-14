using System.ComponentModel.DataAnnotations;

namespace mvc.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Username is required 👤")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Password cannot be empty 🔒")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password 🔁")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match ❌")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Address is required 🏠")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Email is required 📧")]

        [EmailAddress(ErrorMessage = "Please enter a valid email address ✉️")]
        public string Email { get; set; }

       // public string Role { get; set; }
    }
}
