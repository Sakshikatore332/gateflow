
using System;
using System.Collections.Generic;

namespace gateflow.Models;

public partial class GateLog
{
    public int LogId { get; set; }

    public int PassId { get; set; }

    public int GuardId { get; set; }

    public DateTime ExitTime { get; set; }

    public DateTime? EntryTime { get; set; }

    public virtual User Guard { get; set; } = null!;

    public virtual GatePass Pass { get; set; } = null!;
}

