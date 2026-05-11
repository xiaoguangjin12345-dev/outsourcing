using System;
using System.Collections.Generic;

namespace OutsourcingApplication.Models;

public partial class TaskChangeLog
{
    public int ChangeId { get; set; }

    public int TaskId { get; set; }

    public int Pmid { get; set; }

    public int OldHours { get; set; }

    public int NewHours { get; set; }

    public string ChangeReason { get; set; } = null!;

    public DateTime ChangeTime { get; set; }

    public virtual User Pm { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;
}
