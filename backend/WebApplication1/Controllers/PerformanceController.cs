using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OutsourcingApplication.Controllers.Common;
using OutsourcingApplication.DTOs;
using OutsourcingApplication.Models.Common;
using OutsourcingApplication.Services;
using OutsourcingApplication.Services.Interfaces;

namespace OutsourcingApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PerformanceController : BaseController
    {
        private readonly IPerformanceService _performanceService;

        public PerformanceController(IPerformanceService performanceService)
        {
            _performanceService = performanceService;
        }

        // ------------------- 提交评分并结算 ---------------------
        [HttpPut("{id}/score")]
        public ApiResponse<string> SubmitScore([FromRoute] int id, [FromBody] PerformanceScoreDto dto)
        {
            int currentUserId = CurrentUserId;
            int currentRole = CurrentRole;

            bool isSuccess = _performanceService.UpdatePerformanceScore(id, dto, currentUserId, currentRole);

            if (!isSuccess)
            {
                return ApiResponse<string>.Fail(400, "评分失败：记录不存在、无权评价或已结算。");
            }

            return ApiResponse<string>.Success("评分已提交，总分核算完成并已发布。");
        }

        // ------------------- PM/PMO查看待评分绩效列表 ---------------------
        [HttpGet("pending")]
        public ApiResponse<List<PerformancePendingDto>> GetPendingList()
        {
            int userId = CurrentUserId;
            string role = CurrentRole == 2 ? "PM" : "PMO";

            var list = _performanceService.GetPendingPerformances(userId, role);

            return ApiResponse<List<PerformancePendingDto>>.Success(list ?? new List<PerformancePendingDto>());
        }

        // ------------------- 查看已发布绩效 ---------------------
        [HttpGet]
        public ApiResponse<List<PerformanceViewDto>> GetReleasedList([FromQuery] PerformanceQueryDto queryDto)
        {
            int currentUserId = CurrentUserId;
            byte currentUserRole = (byte)CurrentRole;

            var list = _performanceService.GetReleasedPerformances(currentUserId, currentUserRole, queryDto);

            return ApiResponse<List<PerformanceViewDto>>.Success(list ?? new List<PerformanceViewDto>());
        }
    }
}