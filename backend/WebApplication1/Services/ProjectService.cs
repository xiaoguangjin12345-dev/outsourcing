using OutsourcingApplication.DTOs;
using OutsourcingApplication.DTOs.Common;
using OutsourcingApplication.Models;
using OutsourcingApplication.Services.Interfaces;

namespace OutsourcingApplication.Services
{
    public class ProjectService: IProjectService
    {
        private readonly OutsourcingDbContext _context;
        private readonly IPerformanceService _performanceService;
        private readonly IFileService _fileService;

        public ProjectService(OutsourcingDbContext context, IPerformanceService performanceService, IFileService fileService)
        {
            _context = context;
            _performanceService = performanceService;
            _fileService = fileService;
        }
        // ------------------------- 创建项目 实现 ----------------------------------
        public bool CreateProject(int PmId, ProjectCreateDto dto)
        {
            Project project = new Project
            {
                ProjectName = dto.ProjectName,
                ClientName = dto.ClientName,
                ClientEmail = dto.ClientEmail,
                ClientPhone = dto.ClientPhone,
                ProjectDescription = dto.ProjectDescription,
                Budget = dto.Budget,
                Personnel = dto.Personnel,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,

                Pmid = PmId,     
                Status = 1,                 
                CreateTime = DateTime.Now  
            };

            // 添加记录
            _context.Projects.Add(project);
            _context.SaveChanges(); 

            // 处理文件上传
            if (dto.RequirementFile != null)
            {
                string fileUrl = _fileService.SaveFile(dto.RequirementFile, "project", project.ProjectId, "requirement");

                project.RequirementDocUrl = fileUrl;
                _context.SaveChanges();
            }

            return project.ProjectId > 0;
        }

        // ------------------------- 获取项目列表 实现 ------------------------------
        public List<ProjectListDto> GetProjects(int CurrentUserId, int CurrentRole, ProjectQueryDto dto)
        {
            var query = from p in _context.Projects
                        join u in _context.Users on p.Pmid equals u.UserId
                        select new { p, u };

            if (CurrentRole == 2) // PM
            {
                query = query.Where(x => x.p.Pmid == CurrentUserId);
            }
            else if (CurrentRole == 3) // Dev
            {
                return new List<ProjectListDto>();
            }

            // 项目名称
            if (!string.IsNullOrWhiteSpace(dto.ProjectName))
            {
                query = from q in query
                        where q.p.ProjectName.Contains(dto.ProjectName)
                        select q;
            }

            // 项目状态
            if (dto.Statuses != null && dto.Statuses.Any())
            {
                query = from q in query
                        where dto.Statuses.Contains(q.p.Status)
                        select q;
            }

            // 项目经理
            if (dto.PMIDs != null && dto.PMIDs.Any())
            {
                query = from q in query
                        where dto.PMIDs.Contains(q.p.Pmid)
                        select q;
            }

            var resultList = from q in query
                             orderby q.p.CreateTime descending
                             select new ProjectListDto
                             {
                                 ProjectId = q.p.ProjectId,
                                 ProjectName = q.p.ProjectName,
                                 ClientName = q.p.ClientName,
                                 ProjectDescription = q.p.ProjectDescription,
                                 Budget = q.p.Budget,
                                 Status = q.p.Status,
                                 PmName = q.u.RealName,
                                 StartDate = q.p.StartDate,
                                 EndDate = q.p.EndDate,
                                 CreateTime = q.p.CreateTime
                             };

            return resultList.ToList();
        }

        // ------------------------- 查看项目详情 实现 -----------------------------
        public ProjectDetailsDto GetProjectDetails(int id, int currentRole, int currentUserId)
        {
            var query = from p in _context.Projects
                        join u in _context.Users on p.Pmid equals u.UserId
                        where p.ProjectId == id
                        select new { p, u };

            var data = query.FirstOrDefault();

            if (data == null) return null;

            if (currentRole == 2 && data.p.Pmid != currentUserId)
            {
                return null;
            }

            ProjectDetailsDto dto = new ProjectDetailsDto
            {
                ProjectId = data.p.ProjectId,
                ProjectName = data.p.ProjectName,
                ClientName = data.p.ClientName,
                ClientEmail = data.p.ClientEmail,
                ClientPhone = data.p.ClientPhone,
                ProjectDescription = data.p.ProjectDescription,
                Budget = data.p.Budget,
                Personnel = data.p.Personnel,
                RequirementDocUrl = data.p.RequirementDocUrl,
                Status = data.p.Status,
                PmName = data.u.RealName,
                StartDate = data.p.StartDate,
                EndDate = data.p.EndDate,
                CreateTime = data.p.CreateTime,

                TaskCount = data.p.Tasks.Count,
                CompletedTaskCount = (from t in data.p.Tasks
                                      where t.Status == 4 
                                      select t).Count()
            };

            return dto;
        }

        // ------------------------- 修改项目信息 实现 --------------------------
        public string UpdateProject(int id, int currentUserId, ProjectCreateDto dto)
        {
            // 查找项目
            var project = _context.Projects.FirstOrDefault(p => p.ProjectId == id);
            if (project == null) return "找不到该项目，修改失败";

            if (project.Pmid != currentUserId)
            {
                return "越权操作：您不是该项目的负责人，无权修改";
            }

            // 状态：仅在特定阶段（如：待审核1、已驳回2）允许修改
            if (project.Status != 1 && project.Status != 2)
            {
                return "项目当前状态不可修改（已在执行中或已结项）";
            }

            // 处理文件更新
            if (dto.RequirementFile != null)
            {
                // 记录旧路径，用于后面删除
                string oldPath = project.RequirementDocUrl;

                // 保存新文件
                string newUrl = _fileService.SaveFile(dto.RequirementFile, "project", project.ProjectId, "requirement");

                // 更新字段
                project.RequirementDocUrl = newUrl;

                // 清理旧文件
                if (!string.IsNullOrEmpty(oldPath))
                {
                    _fileService.DeleteFile(oldPath);
                }
            }

            project.ProjectName = dto.ProjectName;
            project.ClientName = dto.ClientName;
            project.ClientEmail = dto.ClientEmail;
            project.ClientPhone = dto.ClientPhone;
            project.ProjectDescription = dto.ProjectDescription;
            project.Budget = dto.Budget;
            project.Personnel = dto.Personnel;
            project.StartDate = dto.StartDate;
            project.EndDate = dto.EndDate;

            project.Status = 1;

            int rows = _context.SaveChanges();
            return rows > 0 ? "success" : "数据未发生变动或保存失败";
        }

        // ------------------------- 项目审批 实现 ----------------
        public bool ApproveProject(int projectId, int currentRole, int pmoId, NoticeApproveDto dto)
        {
            // 查询待审批的项目信息
            var project = (from p in _context.Projects
                           where p.ProjectId == projectId
                           select p).FirstOrDefault();

            if (project == null) return false;
            if (currentRole != 1) return false;

            // 更新项目状态
            project.Status = dto.Result ? (byte)3 : (byte)2;

            ProjectApproval approvalRecord = new ProjectApproval
            {
                ProjectId = projectId,
                Pmoid = pmoId,                            
                Result = dto.Result ? (byte)1 : (byte)2,   
                Comment = dto.Reason,            
                ApprovalTime = DateTime.Now      
            };
            _context.ProjectApprovals.Add(approvalRecord);

            // 给项目负责人发送系统通知
            string resultText = dto.Result ? "通过" : "驳回";
            Notice notice = new Notice
            {
                RecieverId = project.Pmid,     
                SenderId = pmoId,                
                Content = $"项目【{project.ProjectName}】审批结果：{resultText}。备注：{dto.Reason}",
                NoticeType = 2,                
                Status = 1,        
                CreateTime = DateTime.Now
            };
            _context.Notices.Add(notice);

            // 提交数据库事务，确保原子性
            int rows = _context.SaveChanges();
            return rows > 0;
        }

        // ------------------------- PM申请结项 实现 ----------------
        public bool ApplyProjectClosure(int projectId, int pmId, ProjectClosureRequestDto dto)
        {
            // 获取项目实体
            var project = (from p in _context.Projects
                               where p.ProjectId == projectId && p.Pmid == pmId
                               select p).FirstOrDefault();

            if (project == null)
            {
                return false;
            }

            if (project.Status != 3)
            {
                return false;
            }

            // 处理文件更新
            if (dto.FinalReportFile != null)
            {
                // 记录旧路径，用于后面删除
                string oldPath = project.FinalReportUrl ?? "";

                // 保存新文件
                string newUrl = _fileService.SaveFile(dto.FinalReportFile, "project", project.ProjectId, "closure");

                // 更新字段
                project.FinalReportUrl = newUrl;

                // 清理旧文件
                if (!string.IsNullOrEmpty(oldPath))
                {
                    _fileService.DeleteFile(oldPath);
                }
            }
            else
            {
                return false;
            }
            project.Status = 4;
            // 提交保存
            int rows = _context.SaveChanges();

            return rows > 0;
        }
        // ------------------------- PMO执行结项 实现 ----------------
        public bool ApproveArchive(int projectId, int currentRole, int pmoId, NoticeApproveDto dto)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // 获取项目实体
                    var project = _context.Projects.FirstOrDefault(p => p.ProjectId == projectId);
                    if (project == null) return false;

                    // 校验：项目下所有任务是否已完成 (Status != 4)
                    bool hasUnfinished = _context.Tasks.Any(t => t.ProjectId == projectId && t.Status != 4);
                    if (hasUnfinished) return false;

                    // 校验：该项目下所有任务对应的绩效是否都已发布 (Status != 2)
                    bool hasUnreleasedPerf = (from pfo in _context.Performances
                                              join t in _context.Tasks on pfo.ObjectId equals t.TaskId
                                              where t.ProjectId == projectId
                                                    && pfo.PerformanceType == 2
                                                    && pfo.Status != 2
                                              select pfo).Any();
                    if (hasUnreleasedPerf) return false;

                    // 处理项目状态变更
                    if (dto.Result == true)
                    {
                        project.Status = 5;
                        project.FinishTime = DateTime.Now;

                        // 项目绩效生成
                        bool perfCreated = _performanceService.CreateProjectPerformance(projectId);
                        if (!perfCreated)
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }
                    else
                    {
                        project.Status = 3;
                    }

                    // 发送通知给PM
                    Notice notice = new Notice();
                    notice.SenderId = pmoId;
                    notice.RecieverId = project.Pmid;
                    notice.NoticeType = 2; 
                    notice.Status = 1;
                    notice.CreateTime = DateTime.Now;

                    if (dto.Result == true)
                    {
                        notice.Content = $"您的项目 [{project.ProjectName}] 已通过结项归档审核，绩效记录已生成。";
                    }
                    else
                    {
                        notice.Content = $"您的项目 [{project.ProjectName}] 结项申请被驳回，原因：{dto.Reason}";
                    }

                    _context.Notices.Add(notice);

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
        // --------------------- 获取项目 标签 实现 ---------------------
        public List<SelectOptionDto> GetApprovedProjectOptions(int userId, int role)
        {
            // 必须是 3,4,5 状态的项目
            var query = _context.Projects.Where(p => p.Status == 3 || p.Status == 4 || p.Status == 5);

            // 根据角色二次过滤
            if (role == 2) // PM
            {
                // 只能看到自己负责的项目
                query = query.Where(p => p.Pmid == userId);
            }
            else if (role == 3) // Dev
            {
                // 只能看到有任务分配给自己的项目
                query = query.Where(p => _context.Tasks.Any(t => t.ProjectId == p.ProjectId && t.DevId == userId));
            }
            // Admin(4) 和 PMO(1) 保持全量

            return query.Select(p => new SelectOptionDto
            {
                Value = p.ProjectId.ToString(),
                Label = p.ProjectName
            }).ToList();
        }
    }
}
