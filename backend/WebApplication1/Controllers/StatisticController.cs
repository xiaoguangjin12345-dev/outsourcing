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
    public class StatisticController : BaseController
    {
        private readonly IStatisticService _statisticService;

        public StatisticController(IStatisticService statisticService)
        {
            _statisticService = statisticService;
        }

        // ------------------------- 进度大盘 -------------------------
        [HttpGet("project-progress")]
        public ApiResponse<List<ProjectProgressDto>> GetProjectProgress([FromQuery] List<int>? projectIds)
        {
            var data = _statisticService.GetProjectProgress(CurrentUserId, (byte)CurrentRole, projectIds);
            return ApiResponse<List<ProjectProgressDto>>.Success(data);
        }

        // ------------------------- 成本偏差雷达 -------------------------
        [HttpGet("work-hours")]
        public ApiResponse<List<WorkHoursDto>> GetWorkHours([FromQuery] string dimension = "project")
        {
            var data = _statisticService.GetWorkHoursAudit(CurrentUserId, (byte)CurrentRole, dimension);
            return ApiResponse<List<WorkHoursDto>>.Success(data);
        }

        // ------------------------- 个体能力画像 -------------------------
        [HttpGet("user-capability/{userId}")]
        public ApiResponse<List<UserCapabilityDto>> GetUserCapability([FromRoute] int userId)
        {
            var data = _statisticService.GetUserCapability(userId, CurrentUserId, (byte)CurrentRole);
            return ApiResponse<List<UserCapabilityDto>>.Success(data);
        }

        // ------------------------- 开发人员效能对标 -------------------------
        [HttpGet("efficiency")]
        public ApiResponse<List<DevEfficiencyDto>> GetEfficiency()
        {
            var data = _statisticService.GetDevEfficiency((byte)CurrentRole);
            return ApiResponse<List<DevEfficiencyDto>>.Success(data);
        }

        // ------------------------- [导出] 开发人员效能 -------------------------
        // 文件导出直接返回二进制流
        [HttpGet("export")]
        public IActionResult ExportStatistic()
        {
            byte[] fileContents = _statisticService.ExportDevEfficiencyToExcel();
            string fileName = $"开发人员效能报表_{DateTime.Now:yyyyMMdd}.xlsx";

            return File(
                fileContents,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }

        // ---------------------- 获取统计分析维度 选项 ---------------------
        [HttpGet("dimensions/options")]
        public ApiResponse<List<SelectOptionDto>> GetAuditDimensions()
        {
            var list = new List<SelectOptionDto>
            {
                new SelectOptionDto { Value = "1", Label = "按项目" },
                new SelectOptionDto { Value = "2", Label = "按开发人员" },
                new SelectOptionDto { Value = "3", Label = "按技术标签" }
            };
            return ApiResponse<List<SelectOptionDto>>.Success(list);
        }

    }
}