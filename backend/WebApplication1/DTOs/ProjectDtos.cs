using System.ComponentModel.DataAnnotations;

namespace OutsourcingApplication.DTOs
{
    public class ProjectCreateDto
    {
        [Required(ErrorMessage = "项目名称是必填的")]
        [StringLength(100, ErrorMessage = "项目名称不能超过100字")]
        public string ProjectName { get; set; } = null!;

        public string? ClientName { get; set; }

        public string? ClientEmail { get; set; }

        public string? ClientPhone { get; set; }

        [StringLength(1000, ErrorMessage = "项目简介不能超过1000字")]
        public string? ProjectDescription { get; set; }
        public decimal? Budget { get; set; }

        public int? Personnel { get; set; }

        public IFormFile? RequirementFile { get; set; }

        [Required(ErrorMessage = "请选择预计开始日期")]
        public DateOnly? StartDate { get; set; }

        [Required(ErrorMessage = "请选择预计结束日期")]
        public DateOnly? EndDate { get; set; }
    }

    public class ProjectQueryDto
    {
        // 模糊查询
        public string? ProjectName { get; set; }

        // 多选模式
        public List<byte>? Statuses { get; set; }

        // 多选模式
        public List<int>? PMIDs { get; set; }
    }
    public class ProjectListDto
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = null!;
        public string? ClientName { get; set; }
        public string? ProjectDescription { get; set; }
        public decimal? Budget { get; set; }
        public byte Status { get; set; }
        public string PmName { get; set; } = null!;
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public DateTime CreateTime { get; set; }
    }
    public class ProjectDetailsDto
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
        public byte Status { get; set; }
        public string PmName { get; set; } = null!;
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public DateTime CreateTime { get; set; }
        public int TaskCount { get; set; }
        public int CompletedTaskCount { get; set; }
    }

    public record ProjectClosureRequestDto
    {
        [Required(ErrorMessage = "必须上传结项报告文件")]
        public IFormFile? FinalReportFile { get; set; }
    }

    public record ProjectAIDto
    {
        public string? ProjectName { get; set; }
        public string? ProjectDescription { get; set; }
    }
}
