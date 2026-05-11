namespace OutsourcingApplication.Models
{
    // 1. 项目进度视图
    public class VProjectProgress
    {
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public byte ProjectStatus { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public decimal? ProgressRate { get; set; } // 改为 decimal?
    }

    // 2. 工时审计视图
    public class VAuditWorkHours
    {
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public int PMID { get; set; }
        public string PMName { get; set; }
        public int AuditCount { get; set; }
        public int? TotalEstimated { get; set; } // 改为 int?
        public int? TotalActual { get; set; }    // 改为 int?
        public int? Variance { get; set; }       // 改为 int?
    }

    // 3. 开发效能视图
    public class VDevEfficiency
    {
        public int UserID { get; set; }
        public string RealName { get; set; }
        public int FinishedTasks { get; set; }
        public decimal? AvgPerformanceScore { get; set; }
        public int TotalWorkHours { get; set; }
    }

    // 4. 技术能力画像视图
    public class VUserCapability
    {
        public int UserID { get; set; }
        public string TagName { get; set; }
        public decimal AvgQuality { get; set; }
        public decimal AvgEfficiency { get; set; }
        public decimal AvgTotal { get; set; }
        public int TaskCount { get; set; }
    }
}
