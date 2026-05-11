using Microsoft.AspNetCore.Authorization;
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
    public class ReviewController : BaseController
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // ------------------------- Dev提交成果，生成初始评审记录 ----------------------
        [HttpPost]
        public ApiResponse<string> SubmitTask([FromForm] ReviewSubmitDto dto)
        {
            // ---------------------- 涉及文件上传，用[FromForm] --------------------
            int currentDevId = CurrentUserId;

            bool isSuccess = _reviewService.CreateReview(currentDevId, dto);

            if (isSuccess == false)
            {
                return ApiResponse<string>.Fail(400, "提交失败：任务不存在、状态不符或您无权提交此任务。");
            }
            return ApiResponse<string>.Success("提交成功！任务已进入待验收状态。");
        }

        [HttpPut("{id}")] // id 是 ReviewId
        public ApiResponse<string> UpdateReview([FromRoute] int id, [FromBody] NoticeApproveDto dto)
        {
            int currentPmId = CurrentUserId;

            bool isSuccess = _reviewService.ProcessReview(currentPmId, id, dto);

            if (!isSuccess)
            {
                return ApiResponse<string>.Fail(400, "评审处理失败：可能单据不存在、权限不足，或任务状态异常。");
            }

            string msg = dto.Result
                ? "验收通过！任务已结项并生成绩效待办。"
                : "指摘成功！任务已退回开发人员，版本号已自动更新。";

            return ApiResponse<string>.Success(msg);
        }

        // ------------------------------- 全角色 查看任务评审列表 ------------------------------
        [HttpGet]
        public ApiResponse<List<ReviewListDto>> GetReviewHistory()
        {
            int currentUserId = CurrentUserId;
            byte currentRole = (byte)CurrentRole;
            var list = _reviewService.GetTaskReview(currentUserId, currentRole);

            return ApiResponse<List<ReviewListDto>>.Success(list ?? new List<ReviewListDto>());
        }
    }
}