using System.ComponentModel.DataAnnotations;

namespace OutsourcingApplication.DTOs
{
    public record TaskCreateDto
    {
        [Required(ErrorMessage = "所属项目不能为空")]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "任务名称不能为空")]
        [StringLength(100)]
        public string TaskName { get; set; }

        [StringLength(2000)]
        public string? TaskDescription { get; set; }

        public List<int>? RequiredSkills { get; set; }

        [Required(ErrorMessage = "预估工时不能为空")]
        [Range(1, 9999)]
        public int EstimatedHours { get; set; }
    }

    public record TaskQueryDto
    {
        public string? TaskName { get; set; }      // 任务名模糊
        public string? ProjectName { get; set; }   // 项目名模糊
        public string? PmName { get; set; }        // 项目经理名模糊
        public string? DevName { get; set; }       // 开发人员名模糊

        public List<int>? ProjectIds { get; set; } // 项目多选
        public List<int>? Statuses { get; set; }   // 状态多选
        public List<int>? Skills { get; set; }     // 技能多选
        public int? ProjectId { get; set; }
    }

    public record TaskListDto
    {
        public int TaskId { get; set; }
        public string TaskName { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string StatusName { get; set; }
        public int EstimatedHours { get; set; }
        public string? RequiredSkills { get; set; }
    }

    public record TaskDetailDto
    {
        public int TaskID { get; set; }
        public string TaskName { get; set; }
        public string? TaskDescription { get; set; }
        public string? RequiredSkills { get; set; }
        public int EstimatedHours { get; set; }
        public int? ActualHours { get; set; }
        public string StatusName { get; set; }
        public DateTime CreateTime { get; set; }
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public int PMID { get; set; }
        public string PMName { get; set; }
        public string? DevName { get; set; }
    }

    public record TaskInviteDto
    {
        [Required]
        public int DevID { get; set; }
    }

    public record TaskHoursUpdateDto
    {
        public int NewEstimatedHours { get; set; }
        public string ChangeReason { get; set; }
    }

}
