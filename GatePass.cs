using System;
using System.Collections.Generic;

namespace gateflow.Models;

public partial class GatePass
{
    public int PassId { get; set; }

    public int UserId { get; set; }

    public string Reason { get; set; } = null!;

    public DateOnly OutDate { get; set; }

    public TimeOnly OutTime { get; set; }

    public DateTime ExpectedReturnTime { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Approval? Approval { get; set; }

    public virtual GateLog? GateLog { get; set; }

    public virtual User User { get; set; } = null!;
}
