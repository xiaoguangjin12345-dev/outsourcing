using System;
using System.Collections.Generic;

namespace OutsourcingApplication.Models;

public partial class TaskApplication
{
    public int ApplicationId { get; set; }

    public int TaskId { get; set; }

    public int Pmid { get; set; }

    public int DevId { get; set; }

    public byte Type { get; set; }

    public byte Status { get; set; }

    public DateTime ApplyTime { get; set; }

    public DateTime? DealTime { get; set; }

    public virtual User Dev { get; set; } = null!;

    public virtual User Pm { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;
}
