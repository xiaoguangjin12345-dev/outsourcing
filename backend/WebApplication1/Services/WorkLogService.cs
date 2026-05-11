using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutsourcingApplication.DTOs;
using OutsourcingApplication.Models;
using OutsourcingApplication.Services.Interfaces;

namespace OutsourcingApplication.Services
{
    public class WorkLogService: IWorkLogService
    {
        private readonly OutsourcingDbContext _context;
        private readonly INoticeService _noticeService;

        public WorkLogService(OutsourcingDbContext context, INoticeService noticeService)
        {
            _context = context;
            _noticeService = noticeService;
        }

        // --------------------------------- Dev记录工时 实现 ----------------------------------------
        public bool RecordWorkLog(int currentUserId, WorkLogSubmitDto dto)
        {
            bool isSuccess = false; // 用于标记事务是否成功
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var task = _context.Tasks.FirstOrDefault(t => t.TaskId == dto.TaskId);

                    // 任务必须存在且状态为进行中(2)
                    if (task == null || task.DevId != currentUserId || task.Status != 2)
                    {
                        return false;
                    }

                    var log = new WorkLog();
                    log.TaskId = dto.TaskId;
                    log.UserId = currentUserId; 
                    log.Description = dto.Description;
                    log.Status = 1;           
                    log.LastTime = DateTime.Now;

                    // 类型转换处理：WorkDate从DateTime转换为DateOnly
                    log.WorkDate = DateOnly.FromDateTime(dto.WorkDate);

                    // 类型转换处理：Hours从decimal转换为int
                    log.Hours = (int)dto.Hours;

                    _context.WorkLogs.Add(log);

                    // 更新任务表的ActualHours
                    int currentTotal = task.ActualHours ?? 0;
                    task.ActualHours = currentTotal + dto.Hours;

                    // 提交
                    _context.SaveChanges();
                    transaction.Commit();

                    isSuccess = true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return false;
                }
            }
            if(isSuccess)
            {
                _noticeService.CheckTaskHoursWarning(dto.TaskId);
            }
            return true;
        }
        // ----------------------------- Dev修改已有日志的工时 实现 -----------------------------------
        public bool UpdateWorkLog(int logId, int currentUserId, WorkLogUpdateDto dto)
        {
            bool isSuccess = false; // 用于标记事务是否成功
            int TaskId = -1;
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // 查找该记录
                    var log = _context.WorkLogs.FirstOrDefault(l => l.LogId == logId && l.UserId == currentUserId);
                    TaskId = log.TaskId;
                    if (log == null)
                    {
                        return false;
                    }

                    // 检查关联任务的状态
                    var task = _context.Tasks.FirstOrDefault(t => t.TaskId == log.TaskId);
                    if (task == null || task.Status >= 3)
                    {
                        return false;
                    }

                    // 更新Task表的总工时
                    int oldHours = log.Hours;
                    int newHours = dto.Hours;

                    int currentTotal = task.ActualHours ?? 0;
                    task.ActualHours = currentTotal - oldHours + newHours;

                    // 更新WorkLog记录
                    log.Hours = newHours;
                    log.Description = dto.Description;
                    log.LastTime = DateTime.Now; // 记录最后修改时间

                    // 统一提交
                    _context.SaveChanges();
                    transaction.Commit();
                    isSuccess = true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return false;
                }
            }
            if (isSuccess)
            {
                _noticeService.CheckTaskHoursWarning(TaskId);
            }
            return true;
        }

        // ----------------------------- Dev删除已有日志 实现 -----------------------------------
        public bool DeleteWorkLog(int logId, int currentUserId)
        {
            bool isSuccess = false; // 用于标记事务是否成功
            int TaskId = -1;
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // 查找该记录
                    var log = _context.WorkLogs.FirstOrDefault(l => l.LogId == logId && l.UserId == currentUserId);
                    TaskId = log.TaskId;
                    if (log == null)
                    {
                        return false;
                    }

                    // 校验工时状态
                    if (log.Status == 2)
                    {
                        return false;
                    }

                    // 校验所属任务状态
                    var task = _context.Tasks.FirstOrDefault(t => t.TaskId == log.TaskId);
                    if (task == null || task.Status >= 3)
                    {
                        return false;
                    }

                    // 更新Task表的累计工时
                    int hoursToSubtract = log.Hours;
                    int currentTotal = task.ActualHours ?? 0;

                    task.ActualHours = currentTotal - hoursToSubtract;

                    // 执行WorkLog记录的物理删除
                    _context.WorkLogs.Remove(log);

                    // 统一保存并提交事务
                    _context.SaveChanges();
                    transaction.Commit();

                    isSuccess = true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return false;
                }
            }
            if (isSuccess)
            {
                _noticeService.CheckTaskHoursWarning(TaskId);
            }
            return true;
        }

        // ----------------------------------- 多维查询工时日志 实现 ----------------------------------------
        public List<WorkLogListDto> GetWorkLogs(WorkLogQueryDto queryDto, int currentUserId, byte currentUserRole)
        {
            // 连表查询
            var query = from l in _context.WorkLogs
                        join t in _context.Tasks on l.TaskId equals t.TaskId
                        join u in _context.Users on l.UserId equals u.UserId
                        join p in _context.Projects on t.ProjectId equals p.ProjectId
                        select new { l, t, u, p };

            // 数据隔离
            if (currentUserRole == 2)
            {
                query = query.Where(q => q.p.Pmid == currentUserId);
            }
            else if (currentUserRole == 3)
            {
                query = query.Where(q => q.l.UserId == currentUserId);
            }
            // PMO与系统管理员可查看全量数据

            // 参数化查询
            // 任务名模糊匹配
            if (!string.IsNullOrWhiteSpace(queryDto.TaskName))
                query = query.Where(q => q.t.TaskName.Contains(queryDto.TaskName));

            // 开发人员名模糊匹配
            if (!string.IsNullOrWhiteSpace(queryDto.UserName))
                query = query.Where(q => q.u.RealName.Contains(queryDto.UserName));

            // 工时日志状态多选
            if (queryDto.Statuses != null && queryDto.Statuses.Any())
                query = query.Where(q => queryDto.Statuses.Contains(q.l.Status));

            // 日期区间查询
            if (queryDto.StartDate.HasValue)
            {
                var start = DateOnly.FromDateTime(queryDto.StartDate.Value);
                query = query.Where(q => q.l.WorkDate >= start);
            }

            if (queryDto.EndDate.HasValue)
            {
                var end = DateOnly.FromDateTime(queryDto.EndDate.Value);
                query = query.Where(q => q.l.WorkDate <= end);
            }

            // ID查询
            if (queryDto.TaskId.HasValue)
                query = query.Where(q => q.l.TaskId == queryDto.TaskId.Value);

            if (queryDto.UserId.HasValue)
                query = query.Where(q => q.l.UserId == queryDto.UserId.Value);


            return query.OrderByDescending(q => q.l.WorkDate)
                        .ThenByDescending(q => q.l.LastTime)
                        .Select(q => new WorkLogListDto
                        {
                            LogId = q.l.LogId,
                            TaskId = q.l.TaskId,
                            TaskName = q.t.TaskName, 
                            UserId = q.l.UserId,
                            UserName = q.u.RealName, 
                            WorkDate = q.l.WorkDate,
                            Hours = q.l.Hours,
                            Description = q.l.Description,
                            LastTime = q.l.LastTime,
                            Status = q.l.Status
                        })
                        .ToList();
        }

        // ----------------------------------- 查询PM预估工时修改 实现 ---------------------------------------- 
        public List<TaskChangeLogListDto> GetTaskChangeLogs(TaskChangeLogQueryDto queryDto, byte currentUserRole)
        {
            // 仅限PMO(1)与Admin(4)访问
            if (currentUserRole != 1 && currentUserRole != 4)
            {
                return new List<TaskChangeLogListDto>();
            }

            // 基础查询
            var query = from log in _context.TaskChangeLogs
                        join t in _context.Tasks on log.TaskId equals t.TaskId
                        join u in _context.Users on log.Pmid equals u.UserId
                        select new { log, t, u };

            // 参数化查询
            // 任务名称模糊查询
            if (!string.IsNullOrWhiteSpace(queryDto.TaskName))
                query = query.Where(q => q.t.TaskName.Contains(queryDto.TaskName));

            // PM姓名模糊查询
            if (!string.IsNullOrWhiteSpace(queryDto.PmName))
                query = query.Where(q => q.u.RealName.Contains(queryDto.PmName));

            // 日期区间过滤
            if (queryDto.StartDate.HasValue)
            {
                query = query.Where(q => q.log.ChangeTime >= queryDto.StartDate.Value.Date);
            }
            if (queryDto.EndDate.HasValue)
            {
                var nextDay = queryDto.EndDate.Value.AddDays(1).Date;
                query = query.Where(q => q.log.ChangeTime < nextDay);
            }

            return query.OrderByDescending(q => q.log.ChangeTime)
                        .Select(q => new TaskChangeLogListDto
                        {
                            ChangeId = q.log.ChangeId,
                            TaskName = q.t.TaskName,
                            PmName = q.u.RealName,
                            OldHours = q.log.OldHours,
                            NewHours = q.log.NewHours,
                            ChangeReason = q.log.ChangeReason,
                            ChangeTime = q.log.ChangeTime
                        }).ToList();
        }
    }
}
