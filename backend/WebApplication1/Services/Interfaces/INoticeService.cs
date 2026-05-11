using OutsourcingApplication.DTOs;

namespace OutsourcingApplication.Services.Interfaces
{
    public interface INoticeService
    {
        // 获取收件箱列表
        List<NoticeDto> GetMyNotices(int currentUserId, NoticeQueryDto queryDto);

        // 获取详情并标记已读
        NoticeDto GetNoticeDetail(int noticeId, int currentUserId);

        // 逻辑删除
        bool SoftDeleteNotice(int noticeId, int currentUserId);
        // 获取未读总数
        int GetUnreadCount(int userId);
        // 预警逻辑
        void CheckTaskHoursWarning(int taskId);
    }
}
