using OutsourcingApplication.Models;

namespace OutsourcingApplication.DTOs
{
    public class WorkLogSubmitDto
    {
        public int TaskId { get; set; }
        public int Hours { get; set; } 
        public string? Description { get; set; }
        public DateTime WorkDate { get; set; } 
    }

    public class WorkLogUpdateDto
    {
        public int Hours { get; set; }
        public string? Description { get; set; }
    }

    public class WorkLogQueryDto
    {
        // 模糊查询：任务名称
        public string? TaskName { get; set; }

        // 模糊查询：开发人员姓名
        public string? UserName { get; set; }

        // 改为区间查询
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // 多选：状态列表 (1-可修改, 2-只读)
        public List<byte>? Statuses { get; set; }

        // 原有的精确 ID 查询保留（可选）
        public int? TaskId { get; set; }
        public int? UserId { get; set; }
    }

    public class WorkLogListDto
    {
        public int LogId { get; set; }
        public int TaskId { get; set; }
        public string TaskName { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public DateOnly WorkDate { get; set; }
        public int Hours { get; set; }
        public string? Description { get; set; }
        public DateTime LastTime { get; set; }
        public byte Status { get; set; }
    }


    public class TaskChangeLogQueryDto
    {
        public string? TaskName { get; set; }  
        public string? PmName { get; set; }   
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class TaskChangeLogListDto
    {
        public int ChangeId { get; set; }
        public string TaskName { get; set; } = null!;
        public string PmName { get; set; } = null!;
        public int OldHours { get; set; }
        public int NewHours { get; set; }
        public string ChangeReason { get; set; } = null!;
        public DateTime ChangeTime { get; set; }
    }
}
