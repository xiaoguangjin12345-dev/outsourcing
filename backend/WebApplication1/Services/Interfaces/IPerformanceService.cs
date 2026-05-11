using OutsourcingApplication.DTOs;

namespace OutsourcingApplication.Services.Interfaces
{
    public interface IPerformanceService
    {
        // 计算任务绩效的 质量分Q 与 工时效率分E
        (decimal Q, decimal E) CalculateTaskMetrics(int version, int estimatedHours, int actualHours);
        // 生成任务绩效记录 实现
        bool CreateTaskPerformance(int taskId);
        // 计算项目绩效的 资源控制率R 与 审计扣分
        (decimal R, decimal AuditPenalty) CalculateProjectMetrics(decimal sumTe, decimal sumTa, int countModify, int kValue);
        // 生成项目绩效记录
        bool CreateProjectPerformance(int projectId);
        // 绩效总分计算
        decimal CalculateTotalScore(byte type, decimal m1, decimal m2, decimal m3);
        // 提交评分并结算
        bool UpdatePerformanceScore(int id, PerformanceScoreDto dto, int currentUserId, int currentRole);
        // PM/PMO查看待评分绩效列表
        List<PerformancePendingDto> GetPendingPerformances(int currentUserId, string role);
        // 查看已发布绩效
        public List<PerformanceViewDto> GetReleasedPerformances(int currentUserId, byte role, PerformanceQueryDto queryDto);
    }
}
