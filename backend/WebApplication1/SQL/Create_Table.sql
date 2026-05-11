CREATE DATABASE [OutsourcingDB];
GO
USE [OutsourcingDB];
GO

-- 1. 用户表 (User)
CREATE TABLE [User] (
    [UserID] INT PRIMARY KEY IDENTITY(1,1),
    [Username] NVARCHAR(50) NOT NULL UNIQUE,
    [Password] NVARCHAR(100) NOT NULL,
    [RealName] NVARCHAR(50) NOT NULL,
    [Role] TINYINT NOT NULL DEFAULT 1,     -- 1-PMO, 2-PM, 3-开发人员, 4-系统管理员
    [Email] NVARCHAR(100) NULL,
    [Phone] NVARCHAR(20) NULL,
    [Status] TINYINT NOT NULL DEFAULT 1,   -- 1-待验证, 2-已验证, 3-未通过
    [ResumeText] NVARCHAR(2000) NULL,
    [Skills] NVARCHAR(100) NULL,           -- 个人技术标签
    [CreateTime] DATETIME NOT NULL DEFAULT GETDATE()
);

-- 2. 项目表 (Project)
CREATE TABLE [Project] (
    [ProjectID] INT PRIMARY KEY IDENTITY(1,1),
    [ProjectName] NVARCHAR(100) NOT NULL,
    [ClientName] NVARCHAR(100) NULL,
    [ClientEmail] NVARCHAR(100) NULL,
    [ClientPhone] NVARCHAR(20) NULL,
    [ProjectDescription] NVARCHAR(2000) NULL,
    [Budget] DECIMAL(18,2) NULL,
    [Personnel] INT NULL,                  -- 预计人力数量
    [RequirementDocUrl] NVARCHAR(500) NULL,
    [PMID] INT NOT NULL,                   -- 外键关联 User.UserID
    [Status] TINYINT NOT NULL DEFAULT 1,   -- 1-待审核, 2-待修改, 3-进行中, 4-待结项, 5-已归档
    [FinalReportUrl] NVARCHAR(500) NULL,
    [CountModify] INT NOT NULL DEFAULT 0,   -- 预估工时修改次数，PM提交预估工时修改时更新
    [StartDate] DATE NULL,
    [EndDate] DATE NULL,
    [CreateTime] DATETIME NOT NULL DEFAULT GETDATE(),
    [FinishTime] DATETIME NULL
);

-- 3. 项目审批表 (Project_Approval)
CREATE TABLE [Project_Approval] (
    [ApprovalID] INT PRIMARY KEY IDENTITY(1,1),
    [ProjectID] INT NOT NULL,              -- 外键关联 Project.ProjectID
    [PMOID] INT NOT NULL,                 -- 外键关联 User.UserID
    [Result] TINYINT NOT NULL,            -- 1-通过, 2-驳回
    [Comment] NVARCHAR(500) NULL,
    [ApprovalTime] DATETIME NOT NULL DEFAULT GETDATE()
);

-- 4. 任务表 (Task)
CREATE TABLE [Task] (
    [TaskID] INT PRIMARY KEY IDENTITY(1,1),
    [ProjectID] INT NOT NULL,
    [TaskName] NVARCHAR(100) NOT NULL,
    [TaskDescription] NVARCHAR(2000) NULL,
    [RequiredSkills] NVARCHAR(100) NULL,
    [DevID] INT NULL,                      -- 外键关联 User.UserID
    [Status] TINYINT NOT NULL DEFAULT 1,   -- 1-待分配, 2-进行中, 3-待验收, 4-已完成
    [Version] INT NOT NULL DEFAULT 1,
    [EstimatedHours] INT NOT NULL,
    [ActualHours] INT NULL,
    [CreateTime] DATETIME NOT NULL DEFAULT GETDATE(),
    [FinishTime] DATETIME NULL
);

-- 5. 任务申请表 (Task_Application)
CREATE TABLE [Task_Application] (
    [ApplicationID] INT PRIMARY KEY IDENTITY(1,1),
    [TaskID] INT NOT NULL,
    [PMID] INT NOT NULL,
    [DevID] INT NOT NULL,
    [Type] TINYINT NOT NULL,               -- 1-PM, 2-开发人员
    [Status] TINYINT NOT NULL DEFAULT 1,   -- 1-待处理, 2-已同意, 3-已失效
    [ApplyTime] DATETIME NOT NULL DEFAULT GETDATE(),
    [DealTime] DATETIME NULL
);

-- 6. 工时日志表 (Work_Log)
CREATE TABLE [Work_Log] (
    [LogID] INT PRIMARY KEY IDENTITY(1,1),
    [TaskID] INT NOT NULL,
    [UserID] INT NOT NULL,
    [WorkDate] DATE NOT NULL,
    [Hours] INT NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [LastTime] DATETIME NOT NULL DEFAULT GETDATE(),
    [Status] TINYINT NOT NULL DEFAULT 1    -- 1-可修改, 2-只读
);

-- 7. 任务评审表 (Task_Review)
CREATE TABLE [Task_Review] (
    [ReviewID] INT PRIMARY KEY IDENTITY(1,1),
    [TaskID] INT NOT NULL,
    [PMID] INT NOT NULL,
    [GitUrl] NVARCHAR(500) NULL,
    [ArchiveUrl] NVARCHAR(500) NULL,
    [DocUrl] NVARCHAR(500) NULL,
    [Version] INT NOT NULL DEFAULT 1,
    [Result] TINYINT NOT NULL DEFAULT 1,        -- 1-待评审, 2-通过, 3-返工
    [Comment] NVARCHAR(500) NULL,
    [ReviewTime] DATETIME NOT NULL DEFAULT GETDATE()
);

-- 8. 绩效表 (Performance)
CREATE TABLE [Performance] (
    [PerformanceID] INT PRIMARY KEY IDENTITY(1,1),
    [PerformanceType] TINYINT NOT NULL,    -- 1-项目, 2-任务
    [ObjectID] INT NULL,                   -- 项目号或任务号
    [EvalUserID] INT NULL,                 -- 评价人
    [BeEvalUserID] INT NOT NULL,           -- 被评价人
    [Metric1] DECIMAL(5,2) NOT NULL,
    [Metric2] DECIMAL(5,2) NOT NULL,
    [Metric3] DECIMAL(5,2) NOT NULL,
    [TotalScore] DECIMAL(5,2) NOT NULL,
    [Comment] NVARCHAR(500) NULL,
    [Status] TINYINT NOT NULL DEFAULT 1,   -- 1-未发布, 2-已发布
    [EvaluateTime] DATETIME NULL
);

-- 9. 通知表 (Notice)
CREATE TABLE [Notice] (
    [NoticeID] INT PRIMARY KEY IDENTITY(1,1),
    [RecieverID] INT NOT NULL,             -- 接收者
    [SenderID] INT NOT NULL,               -- 发送者
    [Content] NVARCHAR(500) NOT NULL,
    [NoticeType] TINYINT NOT NULL,         -- 1-系统, 2-审核, 3-申请, 4-预警, 5-验收, 6-其他
    [Status] TINYINT NOT NULL DEFAULT 1,   -- 1-未读, 2-已读, 3-已删除
    [CreateTime] DATETIME NOT NULL DEFAULT GETDATE()
);

-- 10. 预估工时修改表 (Task_Change_Log)
CREATE TABLE [Task_Change_Log] (
    [ChangeID] INT PRIMARY KEY IDENTITY(1,1),
    [TaskID] INT NOT NULL,
    [PMID] INT NOT NULL,
    [OldHours] INT NOT NULL,
    [NewHours] INT NOT NULL,
    [ChangeReason] NVARCHAR(500) NOT NULL,
    [ChangeTime] DATETIME NOT NULL DEFAULT GETDATE()
);

-- 11. 技能标签字典表
CREATE TABLE [DictTags] (
    [TagID] INT PRIMARY KEY IDENTITY(1,1),
    [TagName] NVARCHAR(50) NOT NULL UNIQUE
);

-- 12. 技能标签关联表 (TargetType: 1-用户, 2-任务)
CREATE TABLE [TagRelation] (
    [RelationID] INT PRIMARY KEY IDENTITY(1,1),
    [TagID] INT NOT NULL,
    [TargetID] INT NOT NULL,
    [TargetType] TINYINT NOT NULL, 
    CONSTRAINT FK_TagRel_Dict FOREIGN KEY ([TagID]) REFERENCES [DictTags] ([TagID])
);
