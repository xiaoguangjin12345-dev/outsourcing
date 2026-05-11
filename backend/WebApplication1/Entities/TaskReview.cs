using System;
using System.Collections.Generic;

namespace OutsourcingApplication.Models;

public partial class TaskReview
{
    public int ReviewId { get; set; }

    public int TaskId { get; set; }

    public int Pmid { get; set; }

    public string? GitUrl { get; set; }

    public string? ArchiveUrl { get; set; }

    public string? DocUrl { get; set; }

    public int Version { get; set; }

    public byte Result { get; set; }

    public string? Comment { get; set; }

    public DateTime ReviewTime { get; set; }

    public virtual User Pm { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;
}
