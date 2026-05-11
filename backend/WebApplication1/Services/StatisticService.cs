using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using System.IO;
using OutsourcingApplication.DTOs;
using OutsourcingApplication.Models;
using OutsourcingApplication.Services.Interfaces;

namespace OutsourcingApplication.Services
{
    public class StatisticService: IStatisticService
    {
        private readonly OutsourcingDbContext _context;
        public StatisticService(OutsourcingDbContext context)
        {
            _context = context;
        }

        // ------------------------- 进度大盘（返回各项目任务完成度比例） 实现 -------------------------
        public List<ProjectProgressDto> GetProjectProgress(int currentUserId, byte role, List<int>? projectIds)
        {
            // 基础查询
            var query = from v in _context.VProjectProgress
                        join p in _context.Projects on v.ProjectID equals p.ProjectId
                        select new { v, p.Pmid };

            // PM 权限过滤
            if (role == 2)
            {
                query = query.Where(q => q.Pmid == currentUserId);
            }
            // Dev 权限过滤
            else if (role == 3)
            {
                var participantProjectIds = _context.Tasks
                                                    .Where(t => t.DevId == currentUserId)
                                                    .Select(t => t.ProjectId)
                                                    .Distinct()
                                                    .ToList();

                // 过滤视图数据
                query = query.Where(q => participantProjectIds.Contains(q.v.ProjectID));
            }

            if (projectIds != null && projectIds.Count > 0)
            {
                query = query.Where(q => projectIds.Contains(q.v.ProjectID));
            }

            // 执行查询并转换
            var rawData = query.ToList();

            return rawData.Select(q => new ProjectProgressDto
            {
                ProjectId = q.v.ProjectID,
                ProjectName = q.v.ProjectName,
                ProjectStatus = q.v.ProjectStatus,
                TotalTasks = q.v.TotalTasks,
                CompletedTasks = q.v.CompletedTasks,
                ProgressRate = q.v.ProgressRate ?? 0
            }).ToList();
        }

        // ------------------------- 成本偏差雷达（预估vs实际的横向对比） 实现 -------------------------
        public List<WorkHoursDto> GetWorkHoursAudit(int currentUserId, byte role, string dimension)
        {
            // 开发人员不予查看
            if (role == 3) return new List<WorkHoursDto>();

            IQueryable<WorkHoursDto> query;

            switch (dimension)
            {
                case "1": // project
                    var projectQuery = _context.VAuditWorkHours.AsQueryable();
                    if (role == 2) projectQuery = projectQuery.Where(p => p.PMID == currentUserId);

                    query = projectQuery.Select(p => new WorkHoursDto
                    {
                        Name = p.ProjectName,
                        TotalEstimated = p.TotalEstimated ?? 0,
                        TotalActual = p.TotalActual ?? 0,
                        Variance = p.Variance ?? 0,
                        VarianceRate = p.TotalEstimated == 0 ? 0 : (decimal)p.Variance / p.TotalEstimated ?? 0
                    });
                    break;

                case "2": // user
                    query = from t in _context.Tasks
                            join u in _context.Users on t.DevId equals u.UserId
                            where t.Status == 4 // 只统计已完工的
                            group t by new { u.UserId, u.RealName } into g
                            select new WorkHoursDto
                            {
                                Name = g.Key.RealName,
                                TotalEstimated = g.Sum(x => x.EstimatedHours),
                                TotalActual = g.Sum(x => x.ActualHours ?? 0),
                                Variance = g.Sum(x => x.ActualHours ?? 0) - g.Sum(x => x.EstimatedHours),
                                VarianceRate = g.Sum(x => x.EstimatedHours) == 0 ? 0 :
                                               (decimal)(g.Sum(x => x.ActualHours ?? 0) - g.Sum(x => x.EstimatedHours)) / g.Sum(x => x.EstimatedHours)
                            };
                    break;

                case "3": // tag
                    query = from tr in _context.TagRelations
                            join dt in _context.DictTags on tr.TagID equals dt.TagID
                            join t in _context.Tasks on tr.TargetID equals t.TaskId
                            where tr.TargetType == 2 && t.Status == 4
                            group t by dt.TagName into g
                            select new WorkHoursDto
                            {
                                Name = g.Key,
                                TotalEstimated = g.Sum(x => x.EstimatedHours),
                                TotalActual = g.Sum(x => x.ActualHours ?? 0),
                                Variance = g.Sum(x => x.ActualHours ?? 0) - g.Sum(x => x.EstimatedHours),
                                VarianceRate = g.Sum(x => x.EstimatedHours) == 0 ? 0 :
                                               (decimal)(g.Sum(x => x.ActualHours ?? 0) - g.Sum(x => x.EstimatedHours)) / g.Sum(x => x.EstimatedHours)
                            };
                    break;

                default:
                    return new List<WorkHoursDto>();
            }

            return query.ToList();
        }

        // ------------------------- 个体能力画像（基于标签聚类后的Q、E分数） 实现 -------------------------
        public List<UserCapabilityDto> GetUserCapability(int targetUserId, int currentUserId, byte role)
        {
            // 权限校
            if (role == 3 && targetUserId != currentUserId)
            {
                return new List<UserCapabilityDto>();
            }

            var data = _context.VUserCapability
                .Where(v => v.UserID == targetUserId)
                .Select(v => new UserCapabilityDto
                {
                    TagName = v.TagName,
                    AvgQuality = (double)v.AvgQuality,
                    AvgEfficiency = (double)v.AvgEfficiency,
                    AvgTotal = (double)v.AvgTotal,
                    TaskCount = v.TaskCount
                })
                .OrderByDescending(v => v.AvgTotal)
                .ToList();

            return data;
        }

        // ------------------------- 开发人员效能对标（PMO视角） 实现 -------------------------
        public List<DevEfficiencyDto> GetDevEfficiency(byte role)
        {
            // 数据隔离
            if (role == 3) return new List<DevEfficiencyDto>();

            // 查询视图
            var query = _context.VDevEfficiency.AsQueryable();

            var data = query.Select(v => new DevEfficiencyDto
            {
                UserId = v.UserID,
                RealName = v.RealName,
                FinishedTasks = v.FinishedTasks,
                AvgPerformanceScore = (double)(v.AvgPerformanceScore ?? 0),
                TotalWorkHours = v.TotalWorkHours
            })
            .OrderByDescending(d => d.AvgPerformanceScore)
            .ToList();

            return data;
        }

        // ------------------------- [导出] 开发人员效能 实现 -------------------------
        public byte[] ExportDevEfficiencyToExcel()
        {
            //  获取数据
            var data = GetDevEfficiency(1);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("开发效能报表");

                // 设置表头
                worksheet.Cell(1, 1).Value = "用户ID";
                worksheet.Cell(1, 2).Value = "姓名";
                worksheet.Cell(1, 3).Value = "完工任务数";
                worksheet.Cell(1, 4).Value = "平均质量分";
                worksheet.Cell(1, 5).Value = "投入总工时(h)";

                var headerRow = worksheet.Range("A1:E1");
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Font.FontColor = XLColor.White;
                headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#409EFF"); 
                headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; 

                // 填充数据
                for (int i = 0; i < data.Count; i++)
                {
                    var item = data[i];
                    int row = i + 2;

                    worksheet.Cell(row, 1).Value = item.UserId;
                    worksheet.Cell(row, 2).Value = item.RealName;
                    worksheet.Cell(row, 3).Value = item.FinishedTasks;
                    worksheet.Cell(row, 4).Value = item.AvgPerformanceScore;
                    worksheet.Cell(row, 5).Value = item.TotalWorkHours;

                    worksheet.Row(row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                // 自动调整列宽
                worksheet.Columns().AdjustToContents();

                // 返回字节数组 (ClosedXML需要通过MemoryStream转换)
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

    }
}
