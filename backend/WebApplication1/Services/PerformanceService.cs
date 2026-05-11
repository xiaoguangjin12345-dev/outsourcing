using OutsourcingApplication.DTOs;
using OutsourcingApplication.Models;
using OutsourcingApplication.Services.Interfaces;

namespace OutsourcingApplication.Services
{
    public class PerformanceService : IPerformanceService
    {
        private readonly OutsourcingDbContext _context;

        public PerformanceService(OutsourcingDbContext context)
        {
            _context = context;
        }

        // ------------------- 计算任务绩效的 质量分Q 与 工时效率分E 实现 ---------------------
        public (decimal Q, decimal E) CalculateTaskMetrics(int version, int estimatedHours, int actualHours)
        {
            // 1. 计算质量分 Q (Metric1)
            // Q = max(60, 100 - (n-1)*10)
            decimal Q = Math.Max(60, 100 - (version - 1) * 10);

            // 2. 计算工时效率分 E (Metric2)
            // E = min(100, 100 * (Te / Ta))
            decimal E;
            if (actualHours > 0)
            {
                E = (100 * ((decimal)estimatedHours / (decimal)actualHours)) > 100 ? 100 : (100 * ((decimal)estimatedHours / (decimal)actualHours));
            }
            else
            {
                E = 100;
            }

            return (Q, E);
        }

        // ------------------- 生成任务绩效记录 实现 -------------------
        public bool CreateTaskPerformance(int taskId)
        {
            try
            {
                // 获取任务信息
                var task = _context.Tasks.FirstOrDefault(t => t.TaskId == taskId);

                if (task == null || task.DevId == null)
                {
                    return false;
                }

                // 获取项目信息
                var project = _context.Projects.FirstOrDefault(p => p.ProjectId == task.ProjectId);
                if (project == null)
                {
                    return false;
                }

                // 计算
                var (scoreQ, scoreE) = CalculateTaskMetrics(
                    task.Version,
                    task.EstimatedHours,
                    task.ActualHours ?? 0
                );

                var perf = new Performance();
                perf.ObjectId = task.TaskId;
                perf.PerformanceType = 2;
                perf.BeEvalUserId = task.DevId.Value;
                perf.EvalUserId = project.Pmid;

                perf.Metric1 = scoreQ;
                perf.Metric2 = scoreE;

                perf.Status = 1; // 1-待评分

                _context.Performances.Add(perf);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // ------------------- 计算项目绩效的 资源控制率R 与 审计扣分 实现 ---------------------
        // 输入：预估总工时、实际总工时、修改次数、K值
        // 输出：资源控制率R、审计扣分项
        public (decimal R, decimal AuditPenalty) CalculateProjectMetrics(decimal sumTe, decimal sumTa, int countModify, int kValue = 2)
        {
            // 计算资源控制率 R
            decimal R = 0;
            if (sumTe > 0)
            {
                // 公式：max(0, 100 * (1 - |ΣTa - ΣTe| / ΣTe))
                decimal deviation = Math.Abs(sumTa - sumTe);
                R = Math.Max(0, 100 * (1 - (deviation / sumTe)));
            }
            else
            {
                R = 0;
            }

            // 计算审计扣分
            // CountModify * K
            decimal auditPenalty = countModify * kValue;

            return (R, auditPenalty);
        }

        // ------------------- 生成项目绩效记录 实现 -------------------
        public bool CreateProjectPerformance(int projectId)
        {
            try
            {
                // 获取项目基本信息
                var project = _context.Projects.FirstOrDefault(p => p.ProjectId == projectId);
                if (project == null) return false;

                // 准备计算所需的数据
                var tasks = _context.Tasks.Where(t => t.ProjectId == projectId).ToList();

                // ΣTe
                decimal sumTe = tasks.Sum(t => (decimal)t.EstimatedHours);
                // ΣTa
                decimal sumTa = tasks.Sum(t => (decimal)(t.ActualHours ?? 0));

                // 计算
                int count = project.CountModify;
                var (scoreR, penalty) = CalculateProjectMetrics(sumTe, sumTa, count);

                var perf = new Performance();
                perf.ObjectId = project.ProjectId;
                perf.PerformanceType = 1;      
                perf.BeEvalUserId = project.Pmid;  

                perf.Metric1 = scoreR;           
                perf.Metric2 = penalty;         

                perf.Status = 1;    

                _context.Performances.Add(perf);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // ----------------------------- 绩效总分计算 实现 --------------------------------
        public decimal CalculateTotalScore(byte type, decimal m1, decimal m2, decimal m3)
        {
            decimal total = 0;

            if (type == 2)
            {
                // 任务绩效
                // Score = Q * 0.5 + E * 0.2 + S * 0.3
                total = (m1 * 0.5m) + (m2 * 0.2m) + (m3 * 0.3m);
            }
            else if (type == 1)
            {
                // 项目绩效
                // Score = R * 0.7 + M * 0.3 - 审计扣分
                total = (m1 * 0.7m) + (m3 * 0.3m) - m2;
            }

            return Math.Max(0, total);
        }

        // ------------------- 提交评分并结算 实现 ---------------------
        public bool UpdatePerformanceScore(int id, PerformanceScoreDto dto, int currentUserId, int currentRole)
        {
            // 查找绩效记录
            var perf = _context.Performances.FirstOrDefault(p => p.PerformanceId == id);

            // 必须是待评分状态
            // 任务级绩效，评价人 必须为 该任务对应的项目经理
            // 项目级绩效，评价人 角色必须为 PMO
            if (perf == null || perf.Status != 1 || 
                (perf.PerformanceType == 2 && perf.EvalUserId != currentUserId) || 
                (perf.PerformanceType == 1 && currentRole != 1))
            {
                return false;
            }

            perf.Metric3 = dto.SubjectiveScore;
            perf.Comment = dto.Comment;
            perf.EvaluateTime = DateTime.Now;

            perf.TotalScore = CalculateTotalScore(
                perf.PerformanceType,
                perf.Metric1,
                perf.Metric2,
                perf.Metric3
            );
            if (perf.PerformanceType == 1)
            {   
                perf.EvalUserId = currentUserId;
            }

            perf.Status = 2;

            _context.SaveChanges();
            return true;
        }
        // ------------------- PM/PMO查看待评分绩效列表 实现 ---------------------
        public List<PerformancePendingDto> GetPendingPerformances(int currentUserId, string role)
        {
            var query = _context.Performances.Where(p => p.Status == 1);

            if (role == "PM")
            {
                // PM 只能看到分配给自己（EvalUserId）的待办记录
                query = query.Where(p => p.EvalUserId == currentUserId);
            }
            else if (role == "PMO")
            {
                // PMO 视角可以看到所有“项目类(Type=1)”且待评分的记录
                query = query.Where(p => p.PerformanceType == 1);
            }
            else
            {
                return new List<PerformancePendingDto>();
            }

            return query.Select(p => new PerformancePendingDto
            {
                PerformanceId = p.PerformanceId,
                PerformanceType = p.PerformanceType,

                ObjectName = p.PerformanceType == 2
                    ? _context.Tasks.FirstOrDefault(t => t.TaskId == p.ObjectId).TaskName
                    : _context.Projects.FirstOrDefault(pr => pr.ProjectId == p.ObjectId).ProjectName,

                BeEvalUserName = p.BeEvalUser.RealName,
                Metric1 = p.Metric1,
                Metric2 = p.Metric2
            }).ToList();
        }

        // ------------------- 查看已发布绩效 实现 ---------------------
        public List<PerformanceViewDto> GetReleasedPerformances(int currentUserId, byte role, PerformanceQueryDto queryDto)
        {
            var query = _context.Performances.Where(p => p.Status == 2).AsQueryable();


            if (role == 3) // Dev
            {
                // 只能看被评价人是自己的记录
                query = query.Where(p => p.BeEvalUserId == currentUserId);
            }
            else if (role == 2) // PM
            {
                query = query.Where(p =>
                    (p.PerformanceType == 1 && _context.Projects.Any(pr => pr.ProjectId == p.ObjectId && pr.Pmid == currentUserId)) ||
                    (p.PerformanceType == 2 && _context.Tasks.Any(t => t.TaskId == p.ObjectId &&
                        _context.Projects.Any(pr => pr.ProjectId == t.ProjectId && pr.Pmid == currentUserId)))
                );
            }


            // 绩效类型多选
            if (queryDto.PerformanceTypes != null && queryDto.PerformanceTypes.Any())
            {
                query = query.Where(p => queryDto.PerformanceTypes.Contains(p.PerformanceType));
            }

            // 被考核人姓名模糊查询
            if (!string.IsNullOrWhiteSpace(queryDto.BeEvalUserName))
            {
                query = query.Where(p => _context.Users.Any(u => u.UserId == p.BeEvalUserId && u.RealName.Contains(queryDto.BeEvalUserName)));
            }

            // 评价时间区间查询
            if (queryDto.StartDate.HasValue)
            {
                query = query.Where(p => p.EvaluateTime >= queryDto.StartDate.Value.Date);
            }
            if (queryDto.EndDate.HasValue)
            {
                var nextDay = queryDto.EndDate.Value.AddDays(1).Date;
                query = query.Where(p => p.EvaluateTime < nextDay);
            }

            // 关联项目/任务名称模糊查询
            if (!string.IsNullOrWhiteSpace(queryDto.ObjectName))
            {
                query = query.Where(p =>
                    (p.PerformanceType == 1 && _context.Projects.Any(pr => pr.ProjectId == p.ObjectId && pr.ProjectName.Contains(queryDto.ObjectName))) ||
                    (p.PerformanceType == 2 && _context.Tasks.Any(t => t.TaskId == p.ObjectId && t.TaskName.Contains(queryDto.ObjectName)))
                );
            }

            var result = query.OrderByDescending(p => p.EvaluateTime).ToList();

            return result.Select(p => new PerformanceViewDto
            {
                PerformanceId = p.PerformanceId,
                Metric1 = p.Metric1,
                Metric2 = p.Metric2,
                Metric3 = p.Metric3,
                TotalScore = p.TotalScore,
                Comment = p.Comment,
                EvaluateTime = p.EvaluateTime,
                ObjectName = p.PerformanceType == 1
                    ? _context.Projects.FirstOrDefault(pr => pr.ProjectId == p.ObjectId)?.ProjectName ?? "项目不存在"
                    : _context.Tasks.FirstOrDefault(t => t.TaskId == p.ObjectId)?.TaskName ?? "任务不存在",
                EvalUserName = _context.Users.FirstOrDefault(u => u.UserId == p.EvalUserId)?.RealName ?? "系统",
                BeEvalUserName = _context.Users.FirstOrDefault(u => u.UserId == p.BeEvalUserId)?.RealName ?? "未知"
            }).ToList();
        }
    }
}
