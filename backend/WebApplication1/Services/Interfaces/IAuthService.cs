using OutsourcingApplication.DTOs;
using OutsourcingApplication.Models;

namespace OutsourcingApplication.Services.Interfaces
{
    public interface IAuthService
    {
        // 登录
        LoginResponseDto? Login(string username, string password);

        // 注册
        bool Register(RegisterRequestDto dto);
    }
}
