using OutsourcingApplication.DTOs;
using OutsourcingApplication.Models;

namespace OutsourcingApplication.Services.Interfaces
{
    public interface IWorkLogService
    {
        // Dev记录工时
        bool RecordWorkLog(int currentDevId, WorkLogSubmitDto dto);
        // Dev修改已有日志的工时
        bool UpdateWorkLog(int logId, int currentUserId, WorkLogUpdateDto dto);
        // Dev删除已有日志
        bool DeleteWorkLog(int logId, int currentUserId);
        // 多维查询工时日志
        List<WorkLogListDto> GetWorkLogs(WorkLogQueryDto query, int currentUserId, byte currentUserRole);
        // 查询PM预估工时修改
        public List<TaskChangeLogListDto> GetTaskChangeLogs(TaskChangeLogQueryDto queryDto, byte currentUserRole);
    }
}
