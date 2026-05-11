using OutsourcingApplication.DTOs;
using OutsourcingApplication.Models;
using OutsourcingApplication.Services.Interfaces;

namespace OutsourcingApplication.Services
{
    public class ReviewService: IReviewService
    {
        private readonly OutsourcingDbContext _context;
        private readonly IPerformanceService _performanceService;
        private readonly IFileService _fileService;

        public ReviewService(OutsourcingDbContext context, IPerformanceService performanceService, IFileService fileService)
        {
            _context = context;
            _performanceService = performanceService;
            _fileService = fileService;
        }


        // ------------------------- Dev提交成果，生成初始评审记录 实现 ----------------
        public bool CreateReview(int devId, ReviewSubmitDto dto)
        {
            // 连表查询
            var taskData = _context.Tasks
                .Where(t => t.TaskId == dto.TaskId && t.DevId == devId)
                .Select(t => new {
                    t.TaskId,
                    t.Version,
                    t.Status,
                    Pmid = t.Project.Pmid
                })
                .FirstOrDefault();

            if (taskData == null || taskData.Status != 2) return false;

            // 自动获取版本路径
            string versionDir = $"v{taskData.Version}";

            // 保存文件
            string archivePath = _fileService.SaveFile(dto.ArchiveFile, "task", taskData.TaskId, $"review/{versionDir}/archive");
            string docPath = _fileService.SaveFile(dto.DocFile, "task", taskData.TaskId, $"review/{versionDir}/doc");

            // 创建评审记录
            TaskReview review = new TaskReview
            {
                TaskId = taskData.TaskId,
                Version = taskData.Version,
                GitUrl = dto.GitUrl,
                ArchiveUrl = archivePath,
                DocUrl = docPath,
                Result = 1, 
                ReviewTime = DateTime.Now,

                Pmid = taskData.Pmid
            };

            // 更新原表任务状态
            var taskEntity = _context.Tasks.Find(taskData.TaskId);
            if (taskEntity != null)
            {
                taskEntity.Status = 3;
            }

            _context.TaskReviews.Add(review);

            return _context.SaveChanges() > 0;
        }
        // -------------------------PM提交任务评审结果 实现----------------------------
        public bool ProcessReview(int pmId, int reviewId, NoticeApproveDto dto)
        {
            // 评审记录
            var review = _context.TaskReviews.FirstOrDefault(r => r.ReviewId == reviewId);
            if (review == null) return false;

            // 对应的任务
            var task = _context.Tasks.FirstOrDefault(t => t.TaskId == review.TaskId);
            if (task == null) return false;

            // 权限检查
            var project = _context.Projects.FirstOrDefault(p => p.ProjectId == task.ProjectId);
            if (project == null || project.Pmid != pmId) return false;

            review.Pmid = pmId;
            review.ReviewTime = DateTime.Now;
            review.Comment = dto.Reason;

            if (dto.Result) 
            {
                review.Result = 2; 
                task.Status = 4; 
                task.FinishTime = DateTime.Now;
                bool perfCreated = _performanceService.CreateTaskPerformance(task.TaskId);
                if (!perfCreated)
                {   
                    return false;
                }
            }
            else 
            {
                review.Result = 3;     
                task.Status = 2;       
                task.Version += 1; 
            }

            // 发送消息通知提醒开发人员
            _context.Notices.Add(new Notice
            {
                SenderId = pmId,
                RecieverId = task.DevId ?? 0,
                NoticeType = 3,
                Content = dto.Result ? $"任务 [{task.TaskName}] 评审已通过！" : $"任务 [{task.TaskName}] 被驳回，原因：{dto.Reason}",
                Status = 1,
                CreateTime = DateTime.Now
            });

            return _context.SaveChanges() > 0;
        }
        // ------------------------------- 全角色 查看任务评审列表 (全量/隔离版) ------------------------------
        public List<ReviewListDto> GetTaskReview(int currentUserId, byte role)
        {
            // 基础查询
            var query = from r in _context.TaskReviews
                        join t in _context.Tasks on r.TaskId equals t.TaskId
                        join p in _context.Projects on t.ProjectId equals p.ProjectId
                        join u in _context.Users on r.Pmid equals u.UserId
                        select new { r, t, p, u };

            // 数据隔离
            if (role == 2)
            {
                query = query.Where(q => q.p.Pmid == currentUserId);
            }
            else if (role == 3)
            {
                query = query.Where(q => q.t.DevId == currentUserId);
            }
            // PMO(1)和Admin(4)查看全系统数据

            return query.OrderByDescending(q => q.r.ReviewTime)
                        .Select(q => new ReviewListDto
                        {
                            ReviewId = q.r.ReviewId,
                            TaskId = q.t.TaskId,
                            TaskName = q.t.TaskName, 
                            Version = q.r.Version,
                            Result = q.r.Result,
                            ResultName = q.r.Result == 2 ? "通过" : "返工",
                            Comment = q.r.Comment,
                            PmName = q.u.RealName,
                            ReviewTime = q.r.ReviewTime,
                            GitUrl = q.r.GitUrl,
                            ArchiveUrl = q.r.ArchiveUrl,
                            DocUrl = q.r.DocUrl
                        })
                        .ToList();
        }
    }
}
