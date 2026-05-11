using System;
using System.Collections.Generic;

namespace OutsourcingApplication.Models;

public partial class WorkLog
{
    public int LogId { get; set; }

    public int TaskId { get; set; }

    public int UserId { get; set; }

    public DateOnly WorkDate { get; set; }

    public int Hours { get; set; }

    public string? Description { get; set; }

    public DateTime LastTime { get; set; }

    public byte Status { get; set; }

    public virtual Task Task { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
