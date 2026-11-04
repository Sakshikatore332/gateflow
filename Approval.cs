using System;
using System.Collections.Generic;

namespace gateflow.Models;

public partial class Approval
{
    public int ApprovalId { get; set; }

    public int PassId { get; set; }

    public int AdminId { get; set; }

    public string Decision { get; set; } = null!;

    public string? Remarks { get; set; }

    public DateTime ApprovalDate { get; set; }

    public virtual User Admin { get; set; } = null!;

    public virtual GatePass Pass { get; set; } = null!;
}
