using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using OutsourcingApplication.DTOs;
using OutsourcingApplication.Models;
using OutsourcingApplication.Services.Interfaces;
using OutsourcingApplication.Services.Utils;

namespace OutsourcingApplication.Services
{
    public class AuthService: IAuthService
    {
        private readonly OutsourcingDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(OutsourcingDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ------------------------用户登录 实现---------------------------
        public LoginResponseDto? Login(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            // 验证用户
            if (user == null || !PasswordHelper.VerifyPassword(password, user.Password))
            {
                return null;
            }

            // 获取token
            var token = GenerateJwtToken(user);

            // 封装完整的信息给前端
            return new LoginResponseDto
            {
                Token = token,
                Role = user.Role,
                RealName = user.RealName ?? "",
                UserId = user.UserId
            };
        }
        // --------------------------用户注册 实现--------------------------
        public bool Register(RegisterRequestDto dto)
        {
            // 唯一性校验
            if (_context.Users.Any(u => u.Username == dto.Username))
            {
                return false;
            }

            // 密码哈希处理
            string hashedPassword = PasswordHelper.HashPassword(dto.Password);

            // 映射实体
            var user = new User
            {
                Username = dto.Username,
                Password = hashedPassword,
                RealName = dto.RealName,
                Role = dto.Role,
                Email = dto.Email,
                Phone = dto.Phone,
                CreateTime = DateTime.Now
            };

            _context.Users.Add(user);
            return _context.SaveChanges() > 0;
        }

        // ------------------------生成Token 实现---------------------------
        private string GenerateJwtToken(User user)
        {
            // 设置 Claim (载荷信息)
            var claims = new List<Claim>
            {
                new Claim("id", user.UserId.ToString()),
                new Claim("username", user.Username),
                new Claim("role", user.Role.ToString()),
                new Claim("realName", user.RealName ?? "")
            };

            // 读取配置并生成 Key
            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 构建Token对象
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["ExpireMinutes"])),
                signingCredentials: creds
            );

            // 序列化为字符串
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
