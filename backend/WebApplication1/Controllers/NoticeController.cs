using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OutsourcingApplication.Controllers.Common;
using OutsourcingApplication.DTOs;
using OutsourcingApplication.Models.Common;
using OutsourcingApplication.Services.Interfaces;

namespace OutsourcingApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NoticeController : BaseController
    {
        private readonly INoticeService _noticeService;

        public NoticeController(INoticeService noticeService)
        {
            _noticeService = noticeService;
        }

        // ------------------------- 获取收件箱列表 -------------------------
        [HttpGet]
        public ApiResponse<List<NoticeDto>> GetInbox([FromQuery] NoticeQueryDto queryDto)
        {
            int userId = CurrentUserId; 
            var list = _noticeService.GetMyNotices(userId, queryDto);
            return ApiResponse<List<NoticeDto>>.Success(list ?? new List<NoticeDto>());
        }

        // ------------------------- 获取详情并标记已读 ------------------------
        [HttpGet("{id}")]
        public ApiResponse<NoticeDto> GetDetail([FromRoute] int id)
        {
            int userId = CurrentUserId;

            var noticeDto = _noticeService.GetNoticeDetail(id, userId);

            if (noticeDto == null)
            {
                return ApiResponse<NoticeDto>.Fail(404, "消息不存在或无权访问");
            }

            return ApiResponse<NoticeDto>.Success(noticeDto);
        }

        // ------------------------- 逻辑删除 ------------------------
        [HttpPut("{id}/delete")]
        public ApiResponse<string> DeleteNotice([FromRoute] int id)
        {
            int userId = CurrentUserId;

            bool success = _noticeService.SoftDeleteNotice(id, userId);

            if (!success)
            {
                return ApiResponse<string>.Fail(400, "操作失败，消息不存在或无权操作");
            }
            return ApiResponse<string>.Success("消息删除成功");
        }

        // ------------------------- 获取未读总数 -------------------
        [HttpGet("unread-count")]
        public ApiResponse<int> GetCount()
        {
            int userId = CurrentUserId;
            int total = _noticeService.GetUnreadCount(userId);

            return ApiResponse<int>.Success(total);
        }
    }
}