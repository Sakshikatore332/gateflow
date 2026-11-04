using System;
using System.Collections.Generic;

namespace gateflow.Models;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string? Department { get; set; }

    public string? Phone { get; set; }

    public virtual ICollection<Approval> Approvals { get; set; } = new List<Approval>();

    public virtual ICollection<GateLog> GateLogs { get; set; } = new List<GateLog>();

    public virtual ICollection<GatePass> GatePasses { get; set; } = new List<GatePass>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
