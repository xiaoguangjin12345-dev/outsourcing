using System;
using System.Collections.Generic;

namespace OutsourcingApplication.Models;

public partial class Performance
{
    public int PerformanceId { get; set; }

    public byte PerformanceType { get; set; }

    public int? ObjectId { get; set; }

    public int? EvalUserId { get; set; }

    public int BeEvalUserId { get; set; }

    public decimal Metric1 { get; set; }

    public decimal Metric2 { get; set; }

    public decimal Metric3 { get; set; }

    public decimal TotalScore { get; set; }

    public string? Comment { get; set; }

    public byte Status { get; set; }

    public DateTime? EvaluateTime { get; set; }

    public virtual User BeEvalUser { get; set; } = null!;

    public virtual User? EvalUser { get; set; }
}
