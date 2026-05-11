using OutsourcingApplication.DTOs;
using OutsourcingApplication.Models;
using OutsourcingApplication.Services.Interfaces;

namespace OutsourcingApplication.Services
{
    public class NoticeService: INoticeService
    {
        private readonly OutsourcingDbContext _context;

        public NoticeService(OutsourcingDbContext context)
        {
            _context = context;
        }

        // ------------------------- 获取收件箱列表 实现 ----------------------
        public List<NoticeDto> GetMyNotices(int currentUserId, NoticeQueryDto queryDto)
        {
            // 基础查询：关联 User 表以获取发送者姓名
            // 且只看发给本人的，且未被删除(Status != 3)
            var query = from n in _context.Notices
                        join u in _context.Users on n.SenderId equals u.UserId into senderJoin
                        from sender in senderJoin.DefaultIfEmpty() // 使用左连接，因为 SenderId=0 可能是系统发送
                        where n.RecieverId == currentUserId && n.Status != 3
                        select new { n, sender };

            // 动态参数筛选
            // 状态多选
            if (queryDto.Statuses != null && queryDto.Statuses.Any())
            {
                query = query.Where(q => queryDto.Statuses.Contains(q.n.Status));
            }
            // 类型多选
            if (queryDto.NoticeTypes != null && queryDto.NoticeTypes.Any())
            {
                query = query.Where(q => queryDto.NoticeTypes.Contains(q.n.NoticeType));
            }
            // 发送人姓名模糊 (处理系统通知id=0的情况)
            if (!string.IsNullOrWhiteSpace(queryDto.SenderName))
            {
                query = query.Where(q => (q.sender != null && q.sender.RealName.Contains(queryDto.SenderName))
                                        || (q.n.SenderId == 0 && "系统".Contains(queryDto.SenderName)));
            }

            // 时间区间过滤
            if (queryDto.StartDate.HasValue)
            {
                query = query.Where(q => q.n.CreateTime >= queryDto.StartDate.Value.Date);
            }
            if (queryDto.EndDate.HasValue)
            {
                var nextDay = queryDto.EndDate.Value.AddDays(1).Date;
                query = query.Where(q => q.n.CreateTime < nextDay);
            }

            // 执行查询并排序
            return query
                .OrderByDescending(q => q.n.CreateTime)
                .Select(q => new NoticeDto
                {
                    NoticeId = q.n.NoticeId,
                    Content = q.n.Content,
                    NoticeType = q.n.NoticeType,
                    Status = q.n.Status,
                    CreateTime = q.n.CreateTime,
                    // 如果 SenderId 是 0，显示为“系统”，否则显示真实姓名
                    SenderName = q.n.SenderId == 0 ? "系统" : (q.sender != null ? q.sender.RealName : "未知")
                })
                .ToList();
        }

        // ------------------------- 获取详情并标记已读 实现 ------------------------
        public NoticeDto GetNoticeDetail(int noticeId, int currentUserId)
        {
            // 获取该条通知，必须满足：是发给本人的、没被逻辑删除
            var notice = _context.Notices
                .FirstOrDefault(n => n.NoticeId == noticeId && n.RecieverId == currentUserId && n.Status != 3);

            if (notice == null) return null;

            if (notice.Status == 1)
            {
                notice.Status = 2;
                _context.SaveChanges();
            }

            // 返回DTO
            return new NoticeDto
            {
                NoticeId = notice.NoticeId,
                Content = notice.Content,
                NoticeType = notice.NoticeType,
                Status = notice.Status,
                CreateTime = notice.CreateTime,
                SenderName = _context.Users.FirstOrDefault(u => u.UserId == notice.SenderId)?.Username ?? "系统"
            };
        }

        // ------------------------- 逻辑删除 实现 ---------------------
        public bool SoftDeleteNotice(int noticeId, int currentUserId)
        {
            var notice = _context.Notices
                .FirstOrDefault(n => n.NoticeId == noticeId && n.RecieverId == currentUserId);

            if (notice == null) return false;

            // 如果已经是3了，说明已经删除过删过，直接返回 true（幂等性）
            if (notice.Status == 3) return true;

            // 执行逻辑删除
            notice.Status = 3;

            return _context.SaveChanges() > 0;
        }

        // ------------------------- 获取未读总数 实现 ----------------
        public int GetUnreadCount(int userId)
        {
            return _context.Notices
                .Count(n => n.RecieverId == userId && n.Status == 1);
        }

        // ------------------------- 预警逻辑 实现 -------------------------
        public void CheckTaskHoursWarning(int taskId)
        {
            // 获取任务实时工时、任务名以及所属项目的 PM
            var taskInfo = (from t in _context.Tasks
                            join p in _context.Projects on t.ProjectId equals p.ProjectId
                            where t.TaskId == taskId
                            select new
                            {
                                t.TaskName,
                                t.ActualHours,
                                t.EstimatedHours,
                                p.Pmid,
                                p.ProjectName
                            }).FirstOrDefault();

            if (taskInfo == null) return;

            // 预警触：Ta > Te (实际 > 预估)
            if (taskInfo.ActualHours > taskInfo.EstimatedHours)
            {
                var notice = new Notice
                {
                    SenderId = 31,                // 0 代表系统自动触发
                    RecieverId = taskInfo.Pmid, 
                    Content = $"【工时预警】项目《{taskInfo.ProjectName}》的任务“{taskInfo.TaskName}”实际工时({taskInfo.ActualHours}h)已超过预估({taskInfo.EstimatedHours}h)。",
                    NoticeType = 4,              // 4: 工时预警类型
                    Status = 0,                  // 0: 未读
                    CreateTime = DateTime.Now
                };

                _context.Notices.Add(notice);
                _context.SaveChanges();
            }
        }
    }
}
