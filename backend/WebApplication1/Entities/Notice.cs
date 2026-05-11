using System;
using System.Collections.Generic;

namespace OutsourcingApplication.Models;

public partial class Notice
{
    public int NoticeId { get; set; }

    public int RecieverId { get; set; }

    public int SenderId { get; set; }

    public string Content { get; set; } = null!;

    public byte NoticeType { get; set; }

    public byte Status { get; set; }

    public DateTime CreateTime { get; set; }

    public virtual User Reciever { get; set; } = null!;

    public virtual User Sender { get; set; } = null!;
}
