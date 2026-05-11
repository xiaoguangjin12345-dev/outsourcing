using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OutsourcingApplication.Controllers.Common;
using OutsourcingApplication.DTOs;
using OutsourcingApplication.DTOs.Common;
using OutsourcingApplication.Models.Common;
using OutsourcingApplication.Services.Interfaces;

namespace OutsourcingApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectController : BaseController
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        // ---------------------------- 创建项目 ---------------------------- 
        [HttpPost]
        [Authorize]
        public ApiResponse<string> CreateProject([FromForm] ProjectCreateDto dto) 
        {
            // ---------------------- 涉及文件上传，用[FromForm] --------------------
            int PmId = CurrentUserId;

            bool success = _projectService.CreateProject(PmId, dto);

            if (!success) return ApiResponse<string>.Fail(400, "立项失败");

            return ApiResponse<string>.Success("申请已提交");
        }

        // ---------------------------- 获取项目列表 ---------------------------- 
        [HttpGet]
        [Authorize]
        public ApiResponse<List<ProjectListDto>> GetProjects([FromQuery] ProjectQueryDto dto)
        {
            int currentUserId = CurrentUserId;
            int currentRole = CurrentRole;
            var list = _projectService.GetProjects(CurrentUserId, CurrentRole, dto);

            return ApiResponse<List<ProjectListDto>>.Success(list);
        }

        // ---------------------------- 查看项目详情 -----------------------------
        [HttpGet("{id}")]
        [Authorize]
        public ApiResponse<ProjectDetailsDto> GetProjectById([FromRoute] int id)
        {
            int currentUserId = CurrentUserId;
            int currentRole = CurrentRole;
            var details = _projectService.GetProjectDetails(id, currentRole, currentUserId);

            if (details == null)
            {
                return ApiResponse<ProjectDetailsDto>.Fail(404, "该项目不存在");
            }
            return ApiResponse<ProjectDetailsDto>.Success(details);
        }

        // ---------------------------- 修改项目信息 ---------------------------
        [HttpPut("{id}")]
        [Authorize]
        public ApiResponse<string> UpdateProject([FromRoute] int id, [FromForm] ProjectCreateDto dto)
        {
            int currentUserId = CurrentUserId;
            // 调用 Service
            string result = _projectService.UpdateProject(id, currentUserId, dto);

            if (result == "success")
            {
                return ApiResponse<string>.Success("项目资料已更新，状态已重置为待审核");
            }
            else
            {
                return ApiResponse<string>.Fail(400, result);
            }
        }

        // ---------------------------- PMO审批立项 ----------------------
        // {id} 指指的是项目的id
        [HttpPost("{id}/approve")]
        [Authorize]
        public ApiResponse<string> ApproveProject([FromRoute] int id, [FromBody] NoticeApproveDto dto)
        {
            int currentRole = CurrentRole;
            int currentPmoId = CurrentUserId;

            // 调用业务层执行审批
            bool isSuccess = _projectService.ApproveProject(id, currentRole, currentPmoId, dto);

            if (!isSuccess)
            {   
                return ApiResponse<string>.Fail(400, "审批失败，您无权操作或项目不存在或系统异常");
            }
            return ApiResponse<string>.Success("审批已完成，结果已实时通知项目经理");
        }

        // ---------------------------- PM申请结项 ----------------------
        [HttpPut("{id}/closure")]
        public ApiResponse<string> ApplyClosure([FromRoute] int id, [FromForm] ProjectClosureRequestDto dto)
        {
            int currentPmId = CurrentUserId;

            bool isSuccess = _projectService.ApplyProjectClosure(id, currentPmId, dto);

            if (isSuccess == false)
            {
                return ApiResponse<string>.Fail(400, "提交结项申请失败：项目不存在或状态不符合结项要求。");
            }
            return ApiResponse<string>.Success("结项报告已上传，申请已提交。");
        }

        // ---------------------------- PMO审批结项 ----------------------
        [HttpPost("{id}/archive")]
        [Authorize]
        public ApiResponse<string> ArchiveProject([FromRoute] int id, [FromBody] NoticeApproveDto dto)
        {
            int currentRole = CurrentRole;
            int currentPmoId = CurrentUserId;

            bool isSuccess = _projectService.ApproveArchive(id, currentRole, currentPmoId, dto);

            if (isSuccess == false)
            {
                return ApiResponse<string>.Fail(400, "归档失败：请确保所有任务已完成且绩效已全部发布。");
            }
            return ApiResponse<string>.Success("项目归档操作已成功执行，已通知相关负责人。");
        }

        // --------------------- 获取项目 标签 ---------------------
        [HttpGet("options")]
        [Authorize]
        public ApiResponse<List<SelectOptionDto>> GetApprovedOptions()
        {
            int currentUserId = CurrentUserId;
            int currentRole = CurrentRole;

            var list = _projectService.GetApprovedProjectOptions(currentUserId, currentRole);
            return ApiResponse<List<SelectOptionDto>>.Success(list);
        }
    }
}