using Microsoft.AspNetCore.Authorization;
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
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // 用户登录
        [HttpPost("login")]
        [AllowAnonymous]
        public ApiResponse<LoginResponseDto> Login([FromBody] LoginRequestDto dto)
        {
            var result = _authService.Login(dto.Username, dto.Password);
            if (result == null)
            {
                return ApiResponse<LoginResponseDto>.Fail(401, "用户名或密码错误");
            }

            return ApiResponse<LoginResponseDto>.Success(result);
        }

        // 用户注册
        [HttpPost("register")]
        [AllowAnonymous]
        public ApiResponse<string> Register([FromBody] RegisterRequestDto dto)
        {
            bool success = _authService.Register(dto);
            if (!success)
            {
                return ApiResponse<string>.Fail(400, "用户名已存在或注册失败");
            }

            return ApiResponse<string>.Success("用户注册成功");
        }
    }
}