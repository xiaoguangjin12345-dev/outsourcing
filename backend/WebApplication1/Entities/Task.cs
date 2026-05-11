using System;
using System.Collections.Generic;

namespace OutsourcingApplication.Models;

public partial class Task
{
    public int TaskId { get; set; }

    public int ProjectId { get; set; }

    public string TaskName { get; set; } = null!;

    public string? TaskDescription { get; set; }

    public string? RequiredSkills { get; set; }

    public int? DevId { get; set; }

    public byte Status { get; set; }

    public int Version { get; set; }

    public int EstimatedHours { get; set; }

    public int? ActualHours { get; set; }

    public DateTime CreateTime { get; set; }

    public DateTime? FinishTime { get; set; }

    public virtual User? Dev { get; set; }

    public virtual Project Project { get; set; } = null!;

    public virtual ICollection<TaskApplication> TaskApplications { get; set; } = new List<TaskApplication>();

    public virtual ICollection<TaskChangeLog> TaskChangeLogs { get; set; } = new List<TaskChangeLog>();

    public virtual ICollection<TaskReview> TaskReviews { get; set; } = new List<TaskReview>();

    public virtual ICollection<WorkLog> WorkLogs { get; set; } = new List<WorkLog>();
}
