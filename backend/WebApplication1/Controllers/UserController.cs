using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OutsourcingApplication.Controllers.Common;
using OutsourcingApplication.DTOs;
using OutsourcingApplication.DTOs.Common;
using OutsourcingApplication.Models.Common; 
using OutsourcingApplication.Services;
using OutsourcingApplication.Services.Interfaces;

namespace OutsourcingApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // 多条件查找用户
        [HttpGet]
        [Authorize]
        public ApiResponse<List<UserDto>> GetUsers([FromQuery] UserQueryDto query)
        {
            // 直接调用service的查询逻辑
            var list = _userService.GetUsers(query);

            return ApiResponse<List<UserDto>>.Success(list);
        }

        // 查单人详情
        [HttpGet("{id}")]
        [Authorize]
        public ApiResponse<UserDetailsDto> GetUserById([FromRoute] int id)
        {  
            var userDetails = _userService.GetUserById(id);
            if (userDetails == null)
            {
                return ApiResponse<UserDetailsDto>.Fail(404, "没找到这个用户");
            }
            return ApiResponse<UserDetailsDto>.Success(userDetails);
        }

        // [系统管理员专用] 对待验证的新用户执行审批
        [HttpPut("{id}/audit")]
        [Authorize]
        public ApiResponse<string> AuditUser([FromRoute] int id, [FromBody] NoticeApproveDto dto)
        {
            int currentAdminId = CurrentUserId;

            bool isSuccess = _userService.AuditUser(id, currentAdminId, dto);

            if (isSuccess == false)
            {
                return ApiResponse<string>.Fail(400, "审核操作失败，请确认用户是否存在。");
            }
            return ApiResponse<string>.Success("审批已提交，结果已通知用户。");
        }

        //------------------------用户修改非关键信息-----------------------------
        [HttpPut("profile")]
        public ApiResponse<string> UpdateProfile([FromBody] UserProfileUpdateDto dto)
        {
            int currentUserId = CurrentUserId;

            bool success = _userService.UpdateUserProfile(currentUserId, dto);

            if (success) return ApiResponse<string>.Success("修改成功");
            return ApiResponse<string>.Fail(400, "修改失败");
        }
        //----------------------获取项目经理 下拉列表 ---------------------
        [HttpGet("pm/options")]
        public ApiResponse<List<SelectOptionDto>> GetProjectManagers()
        {
            var list = _userService.GetProjectManagersInternal();
            return ApiResponse<List<SelectOptionDto>>.Success(list);
        }

        // ---------------------- 获取开发人员 下拉列表 ---------------------
        [HttpGet("dev/options")]
        [Authorize]
        public ApiResponse<List<SelectOptionDto>> GetDevelopers()
        {
            int currentUserId = CurrentUserId;
            int currentRole = CurrentRole;

            var list = _userService.GetDevelopersInternal(currentUserId, currentRole);
            return ApiResponse<List<SelectOptionDto>>.Success(list);
        }
    }
}
