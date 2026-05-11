using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace OutsourcingApplication.Controllers.Common
{
    public class BaseController : ControllerBase
    {
        // 获取ID
        protected int CurrentUserId
        {
            get
            {
                var claim = User.FindFirst("id") ?? User.FindFirst(ClaimTypes.NameIdentifier);
                if (claim != null && int.TryParse(claim.Value, out int id))
                {
                    return id;
                }
                return 0;
            }
        }

        // 获取角色
        protected int CurrentRole
        {
            get
            {
                var claim = User.FindFirst("role") ?? User.FindFirst(ClaimTypes.Role);
                if (claim != null && int.TryParse(claim.Value, out int roleValue))
                {
                    return roleValue;
                }
                return 0;
            }
        }

        // 获取真实姓名
        protected string CurrentRealName
        {
            get
            {
                var claim = User.FindFirst("realName") ??
                            User.FindFirst(ClaimTypes.Name) ??
                            User.FindFirst("username");
                return claim?.Value ?? string.Empty;
            }
        }
    }
}