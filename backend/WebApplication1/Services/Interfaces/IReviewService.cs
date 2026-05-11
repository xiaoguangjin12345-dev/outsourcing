using OutsourcingApplication.DTOs;

namespace OutsourcingApplication.Services.Interfaces
{
    public interface IReviewService
    {
        // Dev提交成果，生成初始评审记录
        bool CreateReview(int currentDevId, ReviewSubmitDto dto);
        // PM提交任务评审结果
        bool ProcessReview(int pmId, int reviewId, NoticeApproveDto dto);
        // 全角色 查看任务评审列表
        List<ReviewListDto> GetTaskReview(int currentUserId, byte role);

    }
}
