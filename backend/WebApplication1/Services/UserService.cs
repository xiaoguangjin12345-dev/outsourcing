using OutsourcingApplication.DTOs;
using OutsourcingApplication.DTOs.Common;
using OutsourcingApplication.Models;
using OutsourcingApplication.Services.Interfaces;

namespace OutsourcingApplication.Services
{
    public class UserService:IUserService
    {
        private readonly OutsourcingDbContext _context;
        private readonly ITagService _tagService;

        public UserService(OutsourcingDbContext context, ITagService tagService)
        {
            _context = context;
            _tagService = tagService;
        }
        // ------------------------标准化多条件查找用户 实现---------------------------
        public List<UserDto> GetUsers(UserQueryDto query)
        {
            var users = _context.Users.AsQueryable();

            // 角色多选过滤
            if (query.Roles != null && query.Roles.Any())
            {
                users = users.Where(u => query.Roles.Contains(u.Role));
            }

            // 状态多选过滤
            if (query.Statuses != null && query.Statuses.Any())
            {
                users = users.Where(u => query.Statuses.Contains(u.Status));
            }

            // 真实姓名模糊查询
            if (!string.IsNullOrWhiteSpace(query.RealName))
            {
                users = users.Where(u => u.RealName.Contains(query.RealName));
            }

            // 技术标签过滤
            if (query.Skills != null && query.Skills.Any())
            {
                users = users.Where(u => _context.TagRelations.Any(r =>
                    r.TargetType == 1 &&
                    r.TargetID == u.UserId &&
                    query.Skills.Contains(r.TagID)));
            }

            var result = users.Select(u => new UserDto
            {
                UserId = u.UserId,
                Username = u.Username,
                RealName = u.RealName,
                Role = u.Role,
                Email = u.Email,
                Phone = u.Phone,
                Skills = u.Skills 
            })
            .ToList();

            return result;
        }

        // -----------------------查单人详情 实现-------------------
        public UserDetailsDto? GetUserById(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null) return null;

            var dto = new UserDetailsDto
            {
                UserId = user.UserId,
                Username = user.Username,
                RealName = user.RealName,
                Role = user.Role,
                Email = user.Email,
                Phone = user.Phone,
                ResumeText = user.ResumeText,
                Skills = user.Skills
            };

            return dto;
        }

        //------------------------[系统管理员专用] 对待验证的新用户执行审批 实现-----------------------------
        public bool AuditUser(int UserId, int adminId, NoticeApproveDto dto)
        {
            var user = (from u in _context.Users
                        where u.UserId == UserId
                        select u).FirstOrDefault();

            if (user == null)
            {
                return false;
            }

            string noticeContent = "";
            if (dto.Result == true)
            {
                user.Status = 2; // 2-已验证
                noticeContent = "您的注册申请已通过审核，欢迎加入！";
            }
            else
            {
                user.Status = 3; // 3-未通过
                noticeContent = "您的申请被驳回。理由：" + dto.Reason;
            }

            Notice newNotice = new Notice
            {
                RecieverId = UserId,       // 接收者是被审核的人
                SenderId = adminId,        // 发送者是管理员自己
                Content = noticeContent,
                NoticeType = 1,
                Status = 0,
                CreateTime = DateTime.Now
            };

            _context.Notices.Add(newNotice);

            // 统一保存
            int rows = _context.SaveChanges();
            return rows > 0;
        }

        //------------------------用户修改非关键信息 实现-----------------------------
        public bool UpdateUserProfile(int userId, UserProfileUpdateDto dto)
        {
            var user = _context.Users.Find(userId);
            if (user == null) return false;

            if (dto.Email != null) user.Email = dto.Email;
            if (dto.Phone != null) user.Phone = dto.Phone;
            if (dto.ResumeText != null) user.ResumeText = dto.ResumeText;

            // 标签处理
            // 如果 dto.SkillTagIds 为 null，不做处理
            if (dto.SkillTagIds != null)
            {
                _tagService.SaveTagRelations(userId, dto.SkillTagIds, 1);
                _context.SaveChanges();
                _tagService.SyncSkillsString(userId, 1);
            }

            // 统一提交
            return _context.SaveChanges() > 0;
        }

        // ------------------------获取项目经理 标签 实现----------------------
        public List<SelectOptionDto> GetProjectManagersInternal()
        {
            var data = _context.Users
                .Where(u => u.Role == 2 && u.Status == 2) // 角色是项目经理(2)且已验证(2)
                .Select(u => new SelectOptionDto
                {
                    Value = u.UserId.ToString(),
                    Label = u.RealName
                })
                .ToList();

            return data;
        }

        // ------------------------ 获取开发人员 标签 实现 ----------------------
        public List<SelectOptionDto> GetDevelopersInternal(int currentUserId, int role)
        {
            // 角色是开发人员(3)且已验证(2)
            var query = _context.Users.Where(u => u.Role == 3 && u.Status == 2);

            if (role == 3) 
            {
                // 如果当前用户角色是开发人员，下拉列表里只能搜到他自己
                query = query.Where(u => u.UserId == currentUserId);
            }

            return query.Select(u => new SelectOptionDto
            {
                Value = u.UserId.ToString(),
                Label = u.RealName
            }).ToList();
        }
    }
}
