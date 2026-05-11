using OutsourcingApplication.DTOs;
using OutsourcingApplication.DTOs.Common;

namespace OutsourcingApplication.Services.Interfaces
{
    public interface ITaskService
    {
        // 创建任务
        bool CreateTask(int currentPmId, TaskCreateDto dto);
        // 查询任务
        List<TaskListDto> GetTaskList(TaskQueryDto queryDto, int currentUserId, byte currentUserRole);
        // 查询具体任务
        public TaskDetailDto? GetTaskById(int id);
        // Dev申请/PM邀请任务
        public bool CreateApplication(int taskId, int currentUserId, byte currentUserRole, TaskInviteDto? dto);
        // 同意任务申请/邀请
        public bool AcceptApplication(int appId);
        // PM修改预估工时
        bool UpdateTaskEstimatedHours(int taskId, int pmId, TaskHoursUpdateDto dto);
        // 查看申请/邀请列表
        List<TaskApplicationListDto> GetTaskApplications(int currentUserId, int role, byte? direction);
        // 任务广场：开发人员专用的待分配查询
        List<TaskListDto> GetTaskSquareList(TaskQueryDto queryDto, byte currentRole);
        // Dev获取任务下拉框 用于填报工时(也为其他角色做好隔离)
        List<SelectOptionDto> GetTaskOptionsByRole(int userId, int role);
    }
}
