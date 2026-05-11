using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutsourcingApplication.Controllers.Common;
using OutsourcingApplication.DTOs;
using OutsourcingApplication.Models;
using OutsourcingApplication.Models.Common;
using OutsourcingApplication.Services.Interfaces;

namespace OutsourcingApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkLogController : BaseController
    {
        private readonly IWorkLogService _workLogService;

        public WorkLogController(IWorkLogService workLogService)
        {
            _workLogService = workLogService;
        }

        // ------------------------------- Dev记录工时 -------------------------------
        [HttpPost]
        public ApiResponse<string> PostLog([FromBody] WorkLogSubmitDto dto)
        {
            int currentDevId = CurrentUserId;

            bool success = _workLogService.RecordWorkLog(currentDevId, dto);

            if (success == false)
            {
                return ApiResponse<string>.Fail(400, "工时填报失败：任务不是进行中状态或权限不足。");
            }
            return ApiResponse<string>.Success("工时填报成功。");
        }

        // ----------------------------- Dev修改已有日志的工时 -----------------------------------
        [HttpPut("{id}")]
        public ApiResponse<string> UpdateLog([FromRoute] int id, [FromBody] WorkLogUpdateDto dto)
        {
            int currentUserId = CurrentUserId;

            bool success = _workLogService.UpdateWorkLog(id, currentUserId, dto);

            if (success == false)
            {
                return ApiResponse<string>.Fail(400, "工时修改失败：记录不存在、权限不足或任务不是进行中状态。");
            }
            return ApiResponse<string>.Success("工时修改成功。");
        }

        // ----------------------------- Dev删除已有日志 -----------------------------------
        [HttpDelete("{id}")]
        public ApiResponse<string> DeleteLog([FromRoute] int id)
        {
            // int currentUserId = 10;
            int currentUserId = CurrentUserId;

            bool isSuccess = _workLogService.DeleteWorkLog(id, currentUserId);

            if (isSuccess == false)
            {
                return ApiResponse<string>.Fail(400, "工时删除失败：记录不存在，或为只读状态。");
            }
            return ApiResponse<string>.Success("工时记录已删除。");
        }

        // ----------------------------------- 多维查询工时日志 --------------------------------------
        [HttpGet]
        public ApiResponse<List<WorkLogListDto>> GetList([FromQuery] WorkLogQueryDto query)
        {
            int currentUserId = CurrentUserId;
            byte currentRole = (byte)CurrentRole;
            var list = _workLogService.GetWorkLogs(query, CurrentUserId, currentRole);

            return ApiResponse<List<WorkLogListDto>>.Success(list ?? new List<WorkLogListDto>());
        }

        // ----------------------------------- 查询PM预估工时修改 ---------------------------------------- 
        [HttpGet("/api/task-change")]
        public ApiResponse<List<TaskChangeLogListDto>> GetAuditLogs([FromQuery] TaskChangeLogQueryDto queryDto)
        {
            byte currnetrole = (byte)CurrentRole;

            var data = _workLogService.GetTaskChangeLogs(queryDto, currnetrole);
            return ApiResponse<List<TaskChangeLogListDto>>.Success(data, "获取任务预估工时修改记录成功");
        }
    }
}