using System.ComponentModel.DataAnnotations;

namespace OutsourcingApplication.DTOs
{
    // 登录表单
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "用户名不能为空")]
        [StringLength(50)]
        public string Username { get; set; }

        [Required(ErrorMessage = "密码不能为空")]
        public string Password { get; set; }
    }
    // 登录返回
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public int Role { get; set; }
        public string RealName { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
    // 注册表单
    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "用户名不能为空")]
        [StringLength(50)]
        public string Username { get; set; }

        [Required(ErrorMessage = "密码不能为空")]
        [MinLength(6, ErrorMessage = "密码至少6位")]
        public string Password { get; set; }

        [Required(ErrorMessage = "确认密码不能为空")]
        [Compare("Password", ErrorMessage = "两次密码输入不一致")] 
        public string Password2 { get; set; }

        [Required(ErrorMessage = "真实姓名不能为空")]
        public string RealName { get; set; }

        [Required(ErrorMessage = "必须选择注册角色")]
        [Range(1, 3, ErrorMessage = "角色选择超出合法范围")]
        public byte Role { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }
    }
}
