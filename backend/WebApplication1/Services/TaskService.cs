using DocumentFormat.OpenXml.Spreadsheet;
using OutsourcingApplication.DTOs;
using OutsourcingApplication.DTOs.Common;
using OutsourcingApplication.Models;
using OutsourcingApplication.Services.Interfaces;
using Task = OutsourcingApplication.Models.Task;

namespace OutsourcingApplication.Services
{
    public class TaskService : ITaskService
    {
        private readonly OutsourcingDbContext _context;
        private readonly ITagService _tagService;

        public TaskService(OutsourcingDbContext context, ITagService tagService)
        {
            _context = context;
            _tagService = tagService;
        }
        // ------------------------- 创建任务 实现 ---------------------
        public bool CreateTask(int currentPmId, TaskCreateDto dto)
        {
            // 基础校验
            var project = _context.Projects.FirstOrDefault(p => p.ProjectId == dto.ProjectId);

            if (project == null || project.Pmid != currentPmId)
            {
                return false;
            }
            // 开启事务
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                Task newTask = new Task();
                newTask.ProjectId = dto.ProjectId;
                newTask.TaskName = dto.TaskName;
                newTask.TaskDescription = dto.TaskDescription;

                newTask.RequiredSkills = "";

                newTask.EstimatedHours = dto.EstimatedHours;
                newTask.Status = 1;
                newTask.Version = 1;
                newTask.CreateTime = DateTime.Now;

                _context.Tasks.Add(newTask);
                // 第一次保存并提交事务，取任务ID
                _context.SaveChanges();

                // 调用 TagService 存储标签关联
                _tagService.SaveTagRelations(newTask.TaskId, dto.RequiredSkills, 2);
                // 第二次保存并提交事务
                _context.SaveChanges();

                // 同步更新 newTask 实体里的 RequiredSkills 字符串字段
                _tagService.SyncSkillsString(newTask.TaskId, 2);
                // 第三次保存并提交事务
                _context.SaveChanges();

                transaction.Commit();
                return true;
            }
            catch (Exception)
            {
                transaction.Rollback();
                return false;
            }
        }
        // ------------------------- 查询任务 实现 ---------------------
        public List<TaskListDto> GetTaskList(TaskQueryDto queryDto, int currentUserId, byte currentUserRole)
        {
            // 基础连接查询
            var query = from t in _context.Tasks
                        join p in _context.Projects on t.ProjectId equals p.ProjectId
                        join pm in _context.Users on p.Pmid equals pm.UserId
                        join dev in _context.Users on t.DevId equals dev.UserId into devJoin
                        from d in devJoin.DefaultIfEmpty()
                        select new { t, p, pm, d };

            // 数据隔离
            if (currentUserRole == 2)
            {
                query = query.Where(q => q.p.Pmid == currentUserId);
            }
            else if (currentUserRole == 3)
            {
                query = query.Where(q => q.t.DevId == currentUserId && q.t.Status != 1);
            }
            // Admin(4) 和 PMO(1)，保持全量数据

            // 参数化查询
            if (queryDto.ProjectId.HasValue)
            {
                query = query.Where(q => q.t.ProjectId == queryDto.ProjectId.Value);
            }

            if (!string.IsNullOrWhiteSpace(queryDto.TaskName))
                query = query.Where(q => q.t.TaskName.Contains(queryDto.TaskName));

            if (!string.IsNullOrWhiteSpace(queryDto.ProjectName))
                query = query.Where(q => q.p.ProjectName.Contains(queryDto.ProjectName));

            if (!string.IsNullOrWhiteSpace(queryDto.PmName))
                query = query.Where(q => q.pm.RealName.Contains(queryDto.PmName));

            if (!string.IsNullOrWhiteSpace(queryDto.DevName))
                query = query.Where(q => q.d != null && q.d.RealName.Contains(queryDto.DevName));

            if (queryDto.Statuses != null && queryDto.Statuses.Any())
                query = query.Where(q => queryDto.Statuses.Contains(q.t.Status));

            if (queryDto.ProjectIds != null && queryDto.ProjectIds.Any())
                query = query.Where(q => queryDto.ProjectIds.Contains(q.t.ProjectId));

            if (queryDto.Skills != null && queryDto.Skills.Any())
            {
                query = query.Where(q => _context.TagRelations.Any(r =>
                    r.TargetType == 2 &&
                    r.TargetID == q.t.TaskId &&
                    queryDto.Skills.Contains(r.TagID)));
            }

            var list = query.Select(q => new TaskListDto
            {
                TaskId = q.t.TaskId,
                TaskName = q.t.TaskName,
                ProjectId = q.t.ProjectId,
                ProjectName = q.p.ProjectName,
                EstimatedHours = q.t.EstimatedHours,
                StatusName = q.t.Status == 1 ? "待分配" :
                                         q.t.Status == 2 ? "进行中" :
                                         q.t.Status == 3 ? "待验收" : "已完成",
                RequiredSkills = q.t.RequiredSkills
            }).ToList();

            return list;
        }

        // ------------------------- 查询具体任务 实现-------------------
        public TaskDetailDto? GetTaskById(int id)
        {
            var query = from t in _context.Tasks
                        join p in _context.Projects on t.ProjectId equals p.ProjectId
                        join u_pm in _context.Users on p.Pmid equals u_pm.UserId
                        join u_dev in _context.Users on t.DevId equals u_dev.UserId into devGroup
                        from dev in devGroup.DefaultIfEmpty()
                        where t.TaskId == id
                        select new TaskDetailDto
                        {
                            TaskID = t.TaskId,
                            TaskName = t.TaskName,
                            TaskDescription = t.TaskDescription,
                            EstimatedHours = t.EstimatedHours,
                            ActualHours = t.ActualHours,
                            CreateTime = t.CreateTime,
                            ProjectID = t.ProjectId,
                            ProjectName = p.ProjectName,
                            PMID = p.Pmid,
                            PMName = u_pm.RealName,
                            DevName = dev != null ? dev.RealName : "尚未分配",
                            StatusName = t.Status == 1 ? "待分配" :
                                         t.Status == 2 ? "进行中" :
                                         t.Status == 3 ? "待验收" : "已完成",
                            RequiredSkills = t.RequiredSkills
                        };

            var dto = query.FirstOrDefault();
            return dto;
        }

        // ------------------------- Dev申请/PM邀请任务 实现-------------------
        public bool CreateApplication(int taskId, int currentUserId, byte currentUserRole, TaskInviteDto? dto)
        {
            // 查询任务信息
            var taskInfo = (from t in _context.Tasks
                            join p in _context.Projects on t.ProjectId equals p.ProjectId
                            where t.TaskId == taskId
                            select new { t, p.Pmid }).FirstOrDefault();

            if (taskInfo == null || taskInfo.t.Status != 1) return false;

            TaskApplication app = new TaskApplication
            {
                TaskId = taskId,
                Pmid = taskInfo.Pmid,
                ApplyTime = DateTime.Now,
                Status = 1
            };

            int receiverId;
            string content;

            if (dto != null && dto.DevID > 0)
            {
                if (currentUserRole != 2 || taskInfo.Pmid != currentUserId) return false;

                app.DevId = dto.DevID;
                app.Type = 1;
                receiverId = dto.DevID;
                content = $"项目经理邀请您加入任务：[{taskInfo.t.TaskName}]";
            }
            else
            {
                if (currentUserRole != 3) return false;

                app.DevId = currentUserId;
                app.Type = 2;
                receiverId = taskInfo.Pmid;
                content = $"有开发人员申请任务：[{taskInfo.t.TaskName}]";

                if (_context.TaskApplications.Any(a => a.TaskId == taskId && a.DevId == currentUserId && a.Status == 1))
                    return false;
            }

            _context.TaskApplications.Add(app);

            // 发送通知
            _context.Notices.Add(new Notice
            {
                SenderId = currentUserId,
                RecieverId = receiverId,
                NoticeType = 3,
                Status = 1,
                Content = content,
                CreateTime = DateTime.Now
            });

            return _context.SaveChanges() > 0;
        }

        // ------------------------- 同意任务申请/邀请 实现-------------------
        public bool AcceptApplication(int appId)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var currentApp = _context.TaskApplications.FirstOrDefault(a => a.ApplicationId == appId);
                    if (currentApp == null || currentApp.Status != 1) return false;

                    var task = _context.Tasks.FirstOrDefault(t => t.TaskId == currentApp.TaskId);
                    if (task == null || task.Status != 1) return false;

                    task.Status = 2;
                    task.DevId = currentApp.DevId;

                    currentApp.Status = 2;

                    var others = _context.TaskApplications
                        .Where(a => a.TaskId == task.TaskId && a.ApplicationId != appId && a.Status == 1)
                        .ToList();

                    foreach (var other in others)
                    {
                        other.Status = 3;
                    }

                    _context.SaveChanges();
                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }

        // -------------------------- PM修改预估工时 实现 -----------------------------------
        public bool UpdateTaskEstimatedHours(int taskId, int pmId, TaskHoursUpdateDto dto)
        {
            var taskData = (from t in _context.Tasks
                            join p in _context.Projects on t.ProjectId equals p.ProjectId
                            where t.TaskId == taskId
                            select new { Task = t, Project = p }).FirstOrDefault();

            if (taskData == null) return false;
            if (taskData.Project.Pmid != pmId) return false;
            if (taskData.Task.Status == 4) return false;

            int originalHours = taskData.Task.EstimatedHours;
            if (originalHours == dto.NewEstimatedHours)
            {
                return true;
            }

            // 触发审计计数
            taskData.Project.CountModify += 1;

            // 更新任务表中的预估工时
            taskData.Task.EstimatedHours = dto.NewEstimatedHours;

            var changeLog = new TaskChangeLog
            {
                TaskId = taskId,
                Pmid = pmId,
                OldHours = originalHours,
                NewHours = dto.NewEstimatedHours,
                ChangeReason = dto.ChangeReason ?? "未填写变更原因",
                ChangeTime = DateTime.Now
            };

            _context.TaskChangeLogs.Add(changeLog);

            return _context.SaveChanges() > 0;
        }

        // -------------------------- 查看申请/邀请列表 实现 ----------------------------
        public List<TaskApplicationListDto> GetTaskApplications(int currentUserId, int role, byte? direction)
        {
            var query = from app in _context.TaskApplications
                        join u in _context.Users on app.DevId equals u.UserId
                        join t in _context.Tasks on app.TaskId equals t.TaskId
                        select new { app, u, t };

            // 权限隔离
            if (role == 3)
            {
                query = query.Where(q => q.app.DevId == currentUserId);

                if (direction == 1)
                    query = query.Where(q => q.app.Type == 2);
                else if (direction == 2)
                    query = query.Where(q => q.app.Type == 1);
            }
            else if (role == 2)
            {
                query = query.Where(q => q.app.Pmid == currentUserId);

                if (direction == 1) 
                    query = query.Where(q => q.app.Type == 1);
                else if (direction == 2) 
                    query = query.Where(q => q.app.Type == 2);
            }
            // PMO/Admin 看全量

            return query.OrderByDescending(q => q.app.ApplyTime)
                        .Select(q => new TaskApplicationListDto
                        {
                            ApplicationID = q.app.ApplicationId,
                            TaskId = q.t.TaskId,
                            TaskName = q.t.TaskName, 
                            DevID = q.app.DevId,
                            DevName = q.u.RealName,
                            DevSkills = q.u.Skills,
                            Type = q.app.Type,
                            Status = q.app.Status,
                            ApplyTime = q.app.ApplyTime
                        }).ToList();
        }

        // -------------------------- 任务广场：开发人员专用的待分配查询 实现----------------------------
        public List<TaskListDto> GetTaskSquareList(TaskQueryDto queryDto, byte currentRole)
        {
            // 如果不是 Dev (3)，直接返回空
            if (currentRole != 3)
            {
                return new List<TaskListDto>();
            }

            // 状态必须为待分配
            var query = from t in _context.Tasks
                        join p in _context.Projects on t.ProjectId equals p.ProjectId
                        join pm in _context.Users on p.Pmid equals pm.UserId
                        where t.Status == 1
                        select new { t, p, pm };

            if (!string.IsNullOrWhiteSpace(queryDto.TaskName))
                query = query.Where(q => q.t.TaskName.Contains(queryDto.TaskName));

            if (!string.IsNullOrWhiteSpace(queryDto.ProjectName))
                query = query.Where(q => q.p.ProjectName.Contains(queryDto.ProjectName));

            if (!string.IsNullOrWhiteSpace(queryDto.PmName))
                query = query.Where(q => q.pm.RealName.Contains(queryDto.PmName));

            if (queryDto.ProjectIds != null && queryDto.ProjectIds.Any())
                query = query.Where(q => queryDto.ProjectIds.Contains(q.t.ProjectId));

            if (queryDto.Skills != null && queryDto.Skills.Any())
            {
                query = query.Where(q => _context.TagRelations.Any(r =>
                    r.TargetType == 2 &&
                    r.TargetID == q.t.TaskId &&
                    queryDto.Skills.Contains(r.TagID)));
            }

            var list = query.Select(q => new TaskListDto
            {
                TaskId = q.t.TaskId,
                TaskName = q.t.TaskName,
                ProjectName = q.p.ProjectName,
                EstimatedHours = q.t.EstimatedHours,
                StatusName = "待分配",
                RequiredSkills = q.t.RequiredSkills
            }).ToList();

            return list;
        }

        // ---------------------- Dev获取任务下拉框 用于填报工时(也为其他角色做好隔离) 实现 -------------------------
        public List<SelectOptionDto> GetTaskOptionsByRole(int userId, int role)
        {
            var query = _context.Tasks.AsQueryable();

            switch (role)
            {
                case 1: // Admin
                case 4: // PMO
                        // 全量：不加过滤
                    break;

                case 2: // 项目经理 (PM)
                        // 只能看到自己负责的项目下的任务
                    query = from t in _context.Tasks
                            join p in _context.Projects on t.ProjectId equals p.ProjectId
                            where p.Pmid == userId
                            select t;
                    break;

                case 3: // 开发人员 (Dev)
                        // 必须是分配给自己的
                        // 状态不能是已完成
                    query = query.Where(t => t.DevId == userId && t.Status != 4);
                    break;

                default:
                    return new List<SelectOptionDto>();
            }

            return query.Select(t => new SelectOptionDto
            {
                Value = t.TaskId.ToString(),
                Label = t.TaskName
            }).ToList();
        }

    }
 }
