using System;
using System.Collections.Generic;

namespace OutsourcingApplication.Models;

public partial class ProjectApproval
{
    public int ApprovalId { get; set; }

    public int ProjectId { get; set; }

    public int Pmoid { get; set; }

    public byte Result { get; set; }

    public string? Comment { get; set; }

    public DateTime ApprovalTime { get; set; }

    public virtual User Pmo { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;
}
