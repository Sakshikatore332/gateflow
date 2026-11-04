
using System.ComponentModel.DataAnnotations;

namespace GateFlow.ViewModels
{
    public class AdminRejectViewModel
    {
        public int PassId { get; set; }

        [Required(ErrorMessage = "Enter rejection reason")]
        [StringLength(255)]
        public string Remarks { get; set; } = "";
    }
}

