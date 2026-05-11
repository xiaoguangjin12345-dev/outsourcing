namespace OutsourcingApplication.DTOs
{
    public class ProjectProgressDto
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int ProjectStatus { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public decimal ProgressRate { get; set; }
    }
    public class WorkHoursDto
    {
        public string Name { get; set; } // 维度名称（项目名、用户名或标签名）
        public int TotalEstimated { get; set; } // 预估总工时
        public int TotalActual { get; set; }    // 实际总工时
        public int Variance { get; set; }       // 偏差值 (实际 - 预估)
        public decimal VarianceRate { get; set; } // 偏差率
    }
    public class UserCapabilityDto
    {
        public string TagName { get; set; }     // 技术标签（如 C#、Vue）
        public double AvgQuality { get; set; }   // 平均质量分 (Metric1)
        public double AvgEfficiency { get; set; } // 平均效率分 (Metric2)
        public double AvgTotal { get; set; }      // 平均综合分
        public int TaskCount { get; set; }       // 在该技术栈下完成的任务数
    }
    public class DevEfficiencyDto
    {
        public int UserId { get; set; }
        public string RealName { get; set; }
        public int FinishedTasks { get; set; }      // 完工总数
        public double AvgPerformanceScore { get; set; } // 平均质量分
        public int TotalWorkHours { get; set; }     // 投入总工时
    }
}
