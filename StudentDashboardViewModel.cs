
using gateflow.Models;

namespace GateFlow.ViewModels
{
    public class StudentDashboardViewModel
    {
        public string StudentName { get; set; } = "";

        public int TotalPasses { get; set; }

        public int PendingPasses { get; set; }

        public int ApprovedPasses { get; set; }

        public int RejectedPasses { get; set; }

        public List<GatePass> RecentPasses { get; set; }
            = new List<GatePass>();
    }
}

