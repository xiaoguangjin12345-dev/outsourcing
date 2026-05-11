using OutsourcingApplication.DTOs;

namespace OutsourcingApplication.Services.Interfaces
{
    public interface IStatisticService
    {
        // 进度大盘（返回各项目任务完成度比例）
        List<ProjectProgressDto> GetProjectProgress(int currentUserId, byte role, List<int>? projectIds);
        // 成本偏差雷达（预估vs实际的横向对比）
        List<WorkHoursDto> GetWorkHoursAudit(int currentUserId, byte role, string dimension);
        // 个体能力画像（基于标签聚类后的Q、E分数）
        public List<UserCapabilityDto> GetUserCapability(int targetUserId, int currentUserId, byte role);
        // 开发人员效能对标（PMO视角）
        public List<DevEfficiencyDto> GetDevEfficiency(byte role);
        // 返回 byte[] 数组，方便 Controller 转化成文件流
        byte[] ExportDevEfficiencyToExcel();
    }
}
