using Microsoft.AspNetCore.Mvc;
using OutsourcingApplication.DTOs;
using OutsourcingApplication.DTOs.Common;
using OutsourcingApplication.Models;

namespace OutsourcingApplication.Services.Interfaces
{
    public interface IUserService
    {
        // 多条件查找用户
        List<UserDto> GetUsers(UserQueryDto query);
        // 查单人详情
        UserDetailsDto GetUserById(int id);

        // [系统管理员专用] 对待验证的新用户执行审批
        bool AuditUser(int UserId, int adminId, NoticeApproveDto dto);
        // 用户修改非关键信息
        bool UpdateUserProfile(int userId, UserProfileUpdateDto dto);
        // 获取项目经理 标签
        List<SelectOptionDto> GetProjectManagersInternal();
        // 获取开发人员 标签
        public List<SelectOptionDto> GetDevelopersInternal(int currentUserId, int role);

    }
}
