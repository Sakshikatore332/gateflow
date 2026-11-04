using System.ComponentModel.DataAnnotations;

namespace GateFlow.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Enter your full name")]
        [StringLength(100)]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Enter your email")]
        [EmailAddress(ErrorMessage = "Enter a valid email")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Select department")]
        public string Department { get; set; } = "";

        [Required(ErrorMessage = "Enter phone number")]
        [RegularExpression(@"^[0-9]{10}$",
            ErrorMessage = "Enter a valid 10-digit number")]
        public string Phone { get; set; } = "";

        [Required(ErrorMessage = "Enter password")]
        [MinLength(8, ErrorMessage = "Minimum 8 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Confirm your password")]
        [Compare("Password",
            ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = "";
    }
}

