using System.ComponentModel.DataAnnotations;

namespace mvc.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is required ")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Password is required ")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
