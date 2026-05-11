using System;
using System.Collections.Generic;

namespace OutsourcingApplication.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string RealName { get; set; } = null!;

    public byte Role { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public byte Status { get; set; }

    public string? ResumeText { get; set; }

    public string? Skills { get; set; }

    public DateTime CreateTime { get; set; }

    public virtual ICollection<Notice> NoticeRecievers { get; set; } = new List<Notice>();

    public virtual ICollection<Notice> NoticeSenders { get; set; } = new List<Notice>();

    public virtual ICollection<Performance> PerformanceBeEvalUsers { get; set; } = new List<Performance>();

    public virtual ICollection<Performance> PerformanceEvalUsers { get; set; } = new List<Performance>();

    public virtual ICollection<ProjectApproval> ProjectApprovals { get; set; } = new List<ProjectApproval>();

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    public virtual ICollection<TaskApplication> TaskApplicationDevs { get; set; } = new List<TaskApplication>();

    public virtual ICollection<TaskApplication> TaskApplicationPms { get; set; } = new List<TaskApplication>();

    public virtual ICollection<TaskChangeLog> TaskChangeLogs { get; set; } = new List<TaskChangeLog>();

    public virtual ICollection<TaskReview> TaskReviews { get; set; } = new List<TaskReview>();

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();

    public virtual ICollection<WorkLog> WorkLogs { get; set; } = new List<WorkLog>();
}
