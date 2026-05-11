namespace OutsourcingApplication.DTOs
{
    public class TagUpdateDto
    {
        public int TargetID { get; set; }      // 用户ID 或 任务ID
        public List<int> TagIDs { get; set; }   // 标签ID数组 [1, 3, 5]
        public byte TargetType { get; set; }   // 1-用户, 2-任务
    }
}
