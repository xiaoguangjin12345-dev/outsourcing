using System.ComponentModel.DataAnnotations;

namespace OutsourcingApplication.DTOs
{
    //各业务 审批结果 通用模板
    public record NoticeApproveDto
    {
        // 审批结果：true代表通过，false代表驳回
        [Required]
        public bool Result { get; set; }

        // 驳回的理由
        public string? Reason { get; set; }
    }
    public class NoticeDto
    {
        public int NoticeId { get; set; }
        public string SenderName { get; set; }  
        public string Content { get; set; }
        public byte NoticeType { get; set; }  
        public byte Status { get; set; }       
        public DateTime CreateTime { get; set; }
    }
    public class NoticeQueryDto
    {
        // 状态多选：1-未读, 2-已读 (status=3将被过滤，代表逻辑删除)
        public List<byte>? Statuses { get; set; }

        // 类型多选：1-系统, 2-审核结果, 3-任务申请, 4-工时预警, 5-验收, 6-其他
        public List<byte>? NoticeTypes { get; set; }

        // 发送者姓名
        public string? SenderName { get; set; }

        // 通知创建起始日期
        public DateTime? StartDate { get; set; }
        // 通知创建截止日期
        public DateTime? EndDate { get; set; }
    }
}
