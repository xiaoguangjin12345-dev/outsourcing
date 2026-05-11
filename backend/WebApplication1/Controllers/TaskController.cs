using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OutsourcingApplication.Controllers.Common;
using OutsourcingApplication.DTOs;
using OutsourcingApplication.DTOs.Common;
using OutsourcingApplication.Models;
using OutsourcingApplication.Models.Common;
using OutsourcingApplication.Services.Interfaces;

namespace OutsourcingApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TaskController : BaseController
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        // ------------------------- 创建任务 --------------------
        [HttpPost]
        public ApiResponse<string> CreateTask([FromBody] TaskCreateDto dto)
        {
            int currentPmId = CurrentUserId;

            bool isSuccess = _taskService.CreateTask(currentPmId, dto);

            if (isSuccess == false)
            {
                // 权限或数据错误统一拦截
                return ApiResponse<string>.Fail(400, "发布失败：项目不存在或您无权操作该项目");
            }
            return ApiResponse<string>.Success("任务发布成功，已关联至项目 ID: " + dto.ProjectId);
        }

        // ------------------------- 参数化查询任务 -------------------
        [HttpGet]
        public ApiResponse<List<TaskListDto>> GetTasks([FromQuery] TaskQueryDto queryDto)
        {
            int currentUserId = CurrentUserId;
            byte currentUserRole = (byte)CurrentRole;
            List<TaskListDto> list = _taskService.GetTaskList(queryDto, currentUserId, currentUserRole);

            return ApiResponse<List<TaskListDto>>.Success(list);
        }

        // ------------------------- 查询具体任务 -------------------
        [HttpGet("{id}")]
        public ApiResponse<TaskDetailDto> GetTaskDetail([FromRoute] int id)
        {
            TaskDetailDto? detail = _taskService.GetTaskById(id);

            if (detail == null)
            {
                return ApiResponse<TaskDetailDto>.Fail(404, "未找到编号为 " + id + " 的任务");
            }

            return ApiResponse<TaskDetailDto>.Success(detail);
        }

        // ------------------------- Dev申请/PM邀请任务 -------------------
        [HttpPost("{id}/applications")]
        public ApiResponse<string> HandleApplication([FromRoute] int id, [FromBody] TaskInviteDto? dto)
        {
            int currentUserId = CurrentUserId;
            byte currentUserRole = (byte)CurrentRole;

            bool isSuccess = _taskService.CreateApplication(id, currentUserId, currentUserRole, dto);

            if (!isSuccess)
            {
                return ApiResponse<string>.Fail(400, "操作失败：权限不足、任务状态错误或重复申请");
            }

            return ApiResponse<string>.Success("意向已成功送达");
        }

        // ------------------------- 同意任务申请/邀请 ------------------
        [HttpPut("applications/{id}")]
        public ApiResponse<string> AcceptApp([FromRoute] int id)
        {
            bool isSuccess = _taskService.AcceptApplication(id);

            if (!isSuccess)
            {
                return ApiResponse<string>.Fail(400, "操作失败：任务已被分配");
            }
            return ApiResponse<string>.Success("操作成功：任务已正式锁定指定开发人员");
        }

        // ------------------------- PM修改预估工时----------------
        [HttpPut("{id}/hours")]
        public ApiResponse<string> UpdateHours([FromRoute] int id, [FromBody] TaskHoursUpdateDto dto)
        {
            int currentPmId = CurrentUserId;
            bool isSuccess = _taskService.UpdateTaskEstimatedHours(id, currentPmId, dto);

            if (!isSuccess)
            {
                return ApiResponse<string>.Fail(400, "修改失败：任务不存在、任务已关闭或您无权操作");
            }
            return ApiResponse<string>.Success("预估工时已更新为：" + dto.NewEstimatedHours);
        }

        // -------------------------- 查看申请/邀请列表 ----------------------------
        [HttpGet("applications")] 
        public ApiResponse<List<TaskApplicationListDto>> GetApplications([FromQuery] byte? direction)
        {
            int currentUserId = CurrentUserId;
            int currentRole = (byte)CurrentRole;

            // 这里的 direction：1-我发出的，2-我收到的 (针对 PM/Dev)
            var list = _taskService.GetTaskApplications(currentUserId, currentRole, direction);

            return ApiResponse<List<TaskApplicationListDto>>.Success(list);
        }

        // -------------------------- 任务广场：开发人员专用的待分配查询 ----------------------------
        [HttpPost("square")]
        public ApiResponse<List<TaskListDto>> GetTaskSquare([FromQuery] TaskQueryDto queryDto)
        {
            byte currentRole = (byte)CurrentRole;
            // 调用 Service，内部会校验 Role
            var data = _taskService.GetTaskSquareList(queryDto, currentRole);
            return ApiResponse<List<TaskListDto>>.Success(data, "获取待分配任务成功");
        }
        // ---------------------- Dev获取任务下拉框 用于填报工时(也为其他角色做好隔离) -------------------------
        [HttpGet("options")]
        public ApiResponse<List<SelectOptionDto>> GetTaskOptions()
        {
            int currentUserId = CurrentUserId;
            int currentRole = CurrentRole;

            var list = _taskService.GetTaskOptionsByRole(currentUserId, currentRole);
            return ApiResponse<List<SelectOptionDto>>.Success(list);
        }

    }
}