
using System.ComponentModel.DataAnnotations;

namespace GateFlow.ViewModels
{
    public class GatePassFormViewModel
    {
        public int PassId { get; set; }

        [Required(ErrorMessage = "Enter reason")]
        [StringLength(255, MinimumLength = 5)]
        public string Reason { get; set; } = "";

        [Required(ErrorMessage = "Select exit date")]
        [DataType(DataType.Date)]
        public DateTime OutDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Select exit time")]
        [DataType(DataType.Time)]
        public TimeSpan? OutTime { get; set; }

        [Required(ErrorMessage = "Select return date and time")]
        public DateTime? ExpectedReturnTime { get; set; }
    }
}

