
using System.ComponentModel.DataAnnotations;

namespace GateFlow.ViewModels
{
    public class AdminStudentEditViewModel
    {
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string Department { get; set; } = "";

        [RegularExpression(@"^[0-9]{10}$",
            ErrorMessage = "Enter 10-digit phone number")]
        public string? Phone { get; set; }
    }
}

