USE [OutsourcingDB];
GO

-- 1. 项目表 (Project) 关联 用户表 (User) -> 确定项目经理
ALTER TABLE [Project]
ADD CONSTRAINT FK_Project_User_PM
FOREIGN KEY ([PMID]) REFERENCES [User] ([UserID]);

-- 2. 项目审批表 (Project_Approval) 关联 项目表 和 用户表
ALTER TABLE [Project_Approval]
ADD CONSTRAINT FK_Approval_Project
FOREIGN KEY ([ProjectID]) REFERENCES [Project] ([ProjectID]);

ALTER TABLE [Project_Approval]
ADD CONSTRAINT FK_Approval_User_PMO
FOREIGN KEY ([PMOID]) REFERENCES [User] ([UserID]);

-- 3. 任务表 (Task) 关联 项目表 和 用户表
ALTER TABLE [Task]
ADD CONSTRAINT FK_Task_Project
FOREIGN KEY ([ProjectID]) REFERENCES [Project] ([ProjectID]);

ALTER TABLE [Task]
ADD CONSTRAINT FK_Task_User_Dev
FOREIGN KEY ([DevID]) REFERENCES [User] ([UserID]);

-- 4. 任务申请表 (Task_Application) 关联 任务、PM和开发
ALTER TABLE [Task_Application]
ADD CONSTRAINT FK_App_Task
FOREIGN KEY ([TaskID]) REFERENCES [Task] ([TaskID]);

ALTER TABLE [Task_Application]
ADD CONSTRAINT FK_App_User_PM
FOREIGN KEY ([PMID]) REFERENCES [User] ([UserID]);

ALTER TABLE [Task_Application]
ADD CONSTRAINT FK_App_User_Dev
FOREIGN KEY ([DevID]) REFERENCES [User] ([UserID]);

-- 5. 工时日志表 (Work_Log) 关联 任务和用户
ALTER TABLE [Work_Log]
ADD CONSTRAINT FK_Log_Task
FOREIGN KEY ([TaskID]) REFERENCES [Task] ([TaskID]);

ALTER TABLE [Work_Log]
ADD CONSTRAINT FK_Log_User
FOREIGN KEY ([UserID]) REFERENCES [User] ([UserID]);

-- 6. 任务评审表 (Task_Review) 关联 任务和PM
ALTER TABLE [Task_Review]
ADD CONSTRAINT FK_Review_Task
FOREIGN KEY ([TaskID]) REFERENCES [Task] ([TaskID]);

ALTER TABLE [Task_Review]
ADD CONSTRAINT FK_Review_User_PM
FOREIGN KEY ([PMID]) REFERENCES [User] ([UserID]);

-- 7. 绩效表 (Performance) 关联 评价人和被评价人
ALTER TABLE [Performance]
ADD CONSTRAINT FK_Perf_User_Eval
FOREIGN KEY ([EvalUserID]) REFERENCES [User] ([UserID]);

ALTER TABLE [Performance]
ADD CONSTRAINT FK_Perf_User_BeEval
FOREIGN KEY ([BeEvalUserID]) REFERENCES [User] ([UserID]);

-- 8. 通知表 (Notice) 关联 接收者和发送者
ALTER TABLE [Notice]
ADD CONSTRAINT FK_Notice_User_Reciever
FOREIGN KEY ([RecieverID]) REFERENCES [User] ([UserID]);

ALTER TABLE [Notice]
ADD CONSTRAINT FK_Notice_User_Sender
FOREIGN KEY ([SenderID]) REFERENCES [User] ([UserID]);

-- 9. 预估工时修改表 (Task_Change_Log) 关联 任务和PM
ALTER TABLE [Task_Change_Log]
ADD CONSTRAINT FK_Change_Task
FOREIGN KEY ([TaskID]) REFERENCES [Task] ([TaskID]);

ALTER TABLE [Task_Change_Log]
ADD CONSTRAINT FK_Change_User_PM
FOREIGN KEY ([PMID]) REFERENCES [User] ([UserID]);
