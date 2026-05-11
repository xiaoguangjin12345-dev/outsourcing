using System.ComponentModel.DataAnnotations;

namespace OutsourcingApplication.Models
{
    public class TagRelation
    {
        [Key]
        public int RelationID { get; set; }
        public int TagID { get; set; }
        public int TargetID { get; set; } // UserId 或 TaskId
        public byte TargetType { get; set; } // 1-用户, 2-任务
    }
}
