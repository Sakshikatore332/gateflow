using System.ComponentModel.DataAnnotations;

namespace GateFlow.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Enter email")]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Enter password")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }
}

