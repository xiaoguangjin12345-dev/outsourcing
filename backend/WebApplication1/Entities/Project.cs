using System;
using System.Collections.Generic;

namespace OutsourcingApplication.Models;

public partial class Project
{
    public int ProjectId { get; set; }

    public string ProjectName { get; set; } = null!;

    public string? ClientName { get; set; }

    public string? ClientEmail { get; set; }

    public string? ClientPhone { get; set; }

    public string? ProjectDescription { get; set; }

    public decimal? Budget { get; set; }

    public int? Personnel { get; set; }

    public string? RequirementDocUrl { get; set; }

    public int Pmid { get; set; }

    public byte Status { get; set; }

    public string? FinalReportUrl { get; set; }
    public int CountModify { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public DateTime CreateTime { get; set; }

    public DateTime? FinishTime { get; set; }

    public virtual User Pm { get; set; } = null!;

    public virtual ICollection<ProjectApproval> ProjectApprovals { get; set; } = new List<ProjectApproval>();

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
