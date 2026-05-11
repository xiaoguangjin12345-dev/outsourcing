USE [OutsourcingDB];
GO

-- 项目进度看板
CREATE OR ALTER VIEW v_ProjectProgress AS
SELECT 
    p.ProjectID, 
    p.ProjectName,
    p.Status AS ProjectStatus,
    COUNT(t.TaskID) AS TotalTasks,
    ISNULL(SUM(CASE WHEN t.Status = 4 THEN 1 ELSE 0 END), 0) AS CompletedTasks,
    ISNULL(CAST(SUM(CASE WHEN t.Status = 4 THEN 1.0 ELSE 0 END) / NULLIF(COUNT(t.TaskID), 0) * 100 AS DECIMAL(5,2)), 0) AS ProgressRate
FROM [Project] p
LEFT JOIN [Task] t ON p.ProjectID = t.ProjectID
GROUP BY p.ProjectID, p.ProjectName, p.Status;
GO

-- 工时偏差分析
CREATE OR ALTER VIEW v_AuditWorkHours AS
SELECT 
    p.ProjectID, 
    p.ProjectName,
    p.PMID,
    u.RealName AS PMName,
    p.CountModify AS AuditCount, -- PM修改工时的次数
    ISNULL(SUM(t.EstimatedHours), 0) AS TotalEstimated,
    ISNULL(SUM(t.ActualHours), 0) AS TotalActual,
    ISNULL(SUM(t.ActualHours), 0) - ISNULL(SUM(t.EstimatedHours), 0) AS Variance
FROM [Project] p
JOIN [User] u ON p.PMID = u.UserID
LEFT JOIN [Task] t ON p.ProjectID = t.ProjectID
GROUP BY p.ProjectID, p.ProjectName, p.PMID, u.RealName, p.CountModify;
GO

-- 开发人员效能
CREATE OR ALTER VIEW v_DevEfficiency AS
SELECT 
    u.UserID, 
    u.RealName,
    COUNT(t.TaskID) AS FinishedTasks,
    AVG(pf.TotalScore) AS AvgPerformanceScore,
    SUM(t.ActualHours) AS TotalWorkHours
FROM [User] u
JOIN [Task] t ON u.UserID = t.DevID
LEFT JOIN [Performance] pf ON t.TaskID = pf.ObjectID AND pf.PerformanceType = 2
WHERE t.Status = 4
GROUP BY u.UserID, u.RealName;
GO

-- 开发人员能力画像
CREATE OR ALTER VIEW v_UserCapability AS
SELECT 
    u.UserID,
    dt.TagName,
    CAST(AVG(CAST(pf.Metric1 AS FLOAT)) AS DECIMAL(10,2)) AS AvgQuality,    -- 平均质量分
    CAST(AVG(CAST(pf.Metric2 AS FLOAT)) AS DECIMAL(10,2)) AS AvgEfficiency, -- 平均效率分
    CAST(AVG(CAST(pf.TotalScore AS FLOAT)) AS DECIMAL(10,2)) AS AvgTotal,
    COUNT(t.TaskID) AS TaskCount -- 该技术标签下已完成的任务总数
FROM [User] u
JOIN [Task] t ON u.UserID = t.DevID 
JOIN [TagRelation] tr ON t.TaskID = tr.TargetID AND tr.TargetType = 2 
JOIN [DictTags] dt ON tr.TagID = dt.TagID
JOIN [Performance] pf ON t.TaskID = pf.ObjectID AND pf.PerformanceType = 2 
WHERE t.Status = 4 
GROUP BY u.UserID, dt.TagName;
GO