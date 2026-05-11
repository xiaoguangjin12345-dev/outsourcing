namespace OutsourcingApplication.DTOs
{
    public class PerformanceScoreDto
    {
        public decimal SubjectiveScore { get; set; } // 对应 Metric3 (S 或 M)
        public string? Comment { get; set; }         // 评价意见
    }
    public class PerformancePendingDto
    {
        public int PerformanceId { get; set; }
        public byte PerformanceType { get; set; } // 1-项目, 2-任务
        public string ObjectName { get; set; }    // 任务名或项目名
        public string BeEvalUserName { get; set; } // 被考核人姓名
        public decimal Metric1 { get; set; }      // Q 或 R
        public decimal Metric2 { get; set; }      // E 或 审计扣分
    }
    public class PerformanceViewDto
    {
        public int PerformanceId { get; set; }

        // --- 核心业务关联信息 ---
        public byte PerformanceType { get; set; }   // 1-项目, 2-任务
        public string ObjectName { get; set; } = null!; // 任务名或项目名

        // --- 人员信息---
        public string BeEvalUserName { get; set; } = null!; // 被考核人姓名
        public string EvalUserName { get; set; } = null!;   // 评价人姓名

        // --- 绩效评分指标 ---
        public decimal Metric1 { get; set; }        // 质量分 Q / 准时度 R
        public decimal Metric2 { get; set; }        // 效率分 E / 审计扣分
        public decimal Metric3 { get; set; }        // 主观分 S / M
        public decimal TotalScore { get; set; }     // 最终得分

        // --- 辅助信息 ---
        public string? Comment { get; set; }        // 评语
        public DateTime? EvaluateTime { get; set; }  // 评价时间
    }
    public class PerformanceQueryDto
    {
        // 绩效类型多选 (1-项目, 2-任务)
        public List<byte>? PerformanceTypes { get; set; }

        // 关联名称模糊 (项目名或任务名)
        public string? ObjectName { get; set; }

        // 被考核人姓名
        public string? BeEvalUserName { get; set; }

        // 评价时间区间
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
