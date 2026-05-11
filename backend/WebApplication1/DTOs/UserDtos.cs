namespace OutsourcingApplication.DTOs
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string RealName { get; set; }
        public byte Role { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Skills { get; set; }
    }

    public class UserQueryDto
    {
        // 修改为 List，支持前端多选 [1, 2]
        public List<byte>? Roles { get; set; }

        // 修改为 List，支持前端多选 [1, 3]
        public List<byte>? Statuses { get; set; }

        // 模糊查询：真实姓名
        public string? RealName { get; set; }

        // 技术标签多选：前端传 [1, 3, 5]
        public List<int>? Skills { get; set; }
    }

    public class UserDetailsDto
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string RealName { get; set; }
        public byte Role { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? ResumeText { get; set; }
        public string? Skills { get; set; }
    }
    public class UserProfileUpdateDto
    {
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? ResumeText { get; set; }
        public List<int>? SkillTagIds { get; set; }
    }
}
