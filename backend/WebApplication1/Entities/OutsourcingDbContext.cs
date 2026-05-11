using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace OutsourcingApplication.Models;

public partial class OutsourcingDbContext : DbContext
{
    public OutsourcingDbContext()
    {
    }

    public OutsourcingDbContext(DbContextOptions<OutsourcingDbContext> options)
        : base(options)
    {
    }
    // --- 实体表 ---
    public virtual DbSet<Notice> Notices { get; set; }

    public virtual DbSet<Performance> Performances { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<ProjectApproval> ProjectApprovals { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<TaskApplication> TaskApplications { get; set; }

    public virtual DbSet<TaskChangeLog> TaskChangeLogs { get; set; }

    public virtual DbSet<TaskReview> TaskReviews { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<WorkLog> WorkLogs { get; set; }
    public virtual DbSet<DictTag> DictTags { get; set; }
    public virtual DbSet<TagRelation> TagRelations { get; set; }

    // --- 统计视图 ---
    public virtual DbSet<VProjectProgress> VProjectProgress { get; set; }
    public virtual DbSet<VAuditWorkHours> VAuditWorkHours { get; set; }
    public virtual DbSet<VDevEfficiency> VDevEfficiency { get; set; }
    public virtual DbSet<VUserCapability> VUserCapability { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=OutsourcingDB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notice>(entity =>
        {
            entity.HasKey(e => e.NoticeId).HasName("PK__Notice__CE83CB85DE98B4DB");

            entity.ToTable("Notice");

            entity.Property(e => e.NoticeId).HasColumnName("NoticeID");
            entity.Property(e => e.Content).HasMaxLength(500);
            entity.Property(e => e.CreateTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RecieverId).HasColumnName("RecieverID");
            entity.Property(e => e.SenderId).HasColumnName("SenderID");
            entity.Property(e => e.Status).HasDefaultValue((byte)1);

            entity.HasOne(d => d.Reciever).WithMany(p => p.NoticeRecievers)
                .HasForeignKey(d => d.RecieverId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notice_User_Reciever");

            entity.HasOne(d => d.Sender).WithMany(p => p.NoticeSenders)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notice_User_Sender");
        });

        modelBuilder.Entity<Performance>(entity =>
        {
            entity.HasKey(e => e.PerformanceId).HasName("PK__Performa__F9606DE1094DF95E");

            entity.ToTable("Performance");

            entity.Property(e => e.PerformanceId).HasColumnName("PerformanceID");
            entity.Property(e => e.BeEvalUserId).HasColumnName("BeEvalUserID");
            entity.Property(e => e.Comment).HasMaxLength(500);
            entity.Property(e => e.EvalUserId).HasColumnName("EvalUserID");
            entity.Property(e => e.EvaluateTime).HasColumnType("datetime");
            entity.Property(e => e.Metric1).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Metric2).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Metric3).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.ObjectId).HasColumnName("ObjectID");
            entity.Property(e => e.Status).HasDefaultValue((byte)1);
            entity.Property(e => e.TotalScore).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.BeEvalUser).WithMany(p => p.PerformanceBeEvalUsers)
                .HasForeignKey(d => d.BeEvalUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Perf_User_BeEval");

            entity.HasOne(d => d.EvalUser).WithMany(p => p.PerformanceEvalUsers)
                .HasForeignKey(d => d.EvalUserId)
                .HasConstraintName("FK_Perf_User_Eval");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.ProjectId).HasName("PK__Project__761ABED0D8CD1A6E");

            entity.ToTable("Project");

            entity.Property(e => e.ProjectId).HasColumnName("ProjectID");
            entity.Property(e => e.Budget).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ClientEmail).HasMaxLength(100);
            entity.Property(e => e.ClientName).HasMaxLength(100);
            entity.Property(e => e.ClientPhone).HasMaxLength(20);
            entity.Property(e => e.CreateTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FinalReportUrl).HasMaxLength(500);
            entity.Property(e => e.FinishTime).HasColumnType("datetime");
            entity.Property(e => e.Pmid).HasColumnName("PMID");
            entity.Property(e => e.ProjectDescription).HasMaxLength(2000);
            entity.Property(e => e.ProjectName).HasMaxLength(100);
            entity.Property(e => e.RequirementDocUrl).HasMaxLength(500);
            entity.Property(e => e.Status).HasDefaultValue((byte)1);

            entity.HasOne(d => d.Pm).WithMany(p => p.Projects)
                .HasForeignKey(d => d.Pmid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Project_User_PM");
        });

        modelBuilder.Entity<ProjectApproval>(entity =>
        {
            entity.HasKey(e => e.ApprovalId).HasName("PK__Project___328477D4806229BF");

            entity.ToTable("Project_Approval");

            entity.Property(e => e.ApprovalId).HasColumnName("ApprovalID");
            entity.Property(e => e.ApprovalTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Comment).HasMaxLength(500);
            entity.Property(e => e.Pmoid).HasColumnName("PMOID");
            entity.Property(e => e.ProjectId).HasColumnName("ProjectID");

            entity.HasOne(d => d.Pmo).WithMany(p => p.ProjectApprovals)
                .HasForeignKey(d => d.Pmoid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Approval_User_PMO");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectApprovals)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Approval_Project");
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.TaskId).HasName("PK__Task__7C6949D1F28D5EE4");

            entity.ToTable("Task");

            entity.Property(e => e.TaskId).HasColumnName("TaskID");
            entity.Property(e => e.CreateTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DevId).HasColumnName("DevID");
            entity.Property(e => e.FinishTime).HasColumnType("datetime");
            entity.Property(e => e.ProjectId).HasColumnName("ProjectID");
            entity.Property(e => e.RequiredSkills).HasMaxLength(100);
            entity.Property(e => e.Status).HasDefaultValue((byte)1);
            entity.Property(e => e.TaskDescription).HasMaxLength(2000);
            entity.Property(e => e.TaskName).HasMaxLength(100);
            entity.Property(e => e.Version).HasDefaultValue(1);

            entity.HasOne(d => d.Dev).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.DevId)
                .HasConstraintName("FK_Task_User_Dev");

            entity.HasOne(d => d.Project).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Task_Project");
        });

        modelBuilder.Entity<TaskApplication>(entity =>
        {
            entity.HasKey(e => e.ApplicationId).HasName("PK__Task_App__C93A4F790358FA7B");

            entity.ToTable("Task_Application");

            entity.Property(e => e.ApplicationId).HasColumnName("ApplicationID");
            entity.Property(e => e.ApplyTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DealTime).HasColumnType("datetime");
            entity.Property(e => e.DevId).HasColumnName("DevID");
            entity.Property(e => e.Pmid).HasColumnName("PMID");
            entity.Property(e => e.Status).HasDefaultValue((byte)1);
            entity.Property(e => e.TaskId).HasColumnName("TaskID");

            entity.HasOne(d => d.Dev).WithMany(p => p.TaskApplicationDevs)
                .HasForeignKey(d => d.DevId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_App_User_Dev");

            entity.HasOne(d => d.Pm).WithMany(p => p.TaskApplicationPms)
                .HasForeignKey(d => d.Pmid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_App_User_PM");

            entity.HasOne(d => d.Task).WithMany(p => p.TaskApplications)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_App_Task");
        });

        modelBuilder.Entity<TaskChangeLog>(entity =>
        {
            entity.HasKey(e => e.ChangeId).HasName("PK__Task_Cha__0E05C5B7C57DFEEA");

            entity.ToTable("Task_Change_Log");

            entity.Property(e => e.ChangeId).HasColumnName("ChangeID");
            entity.Property(e => e.ChangeReason).HasMaxLength(500);
            entity.Property(e => e.ChangeTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Pmid).HasColumnName("PMID");
            entity.Property(e => e.TaskId).HasColumnName("TaskID");

            entity.HasOne(d => d.Pm).WithMany(p => p.TaskChangeLogs)
                .HasForeignKey(d => d.Pmid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Change_User_PM");

            entity.HasOne(d => d.Task).WithMany(p => p.TaskChangeLogs)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Change_Task");
        });

        modelBuilder.Entity<TaskReview>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__Task_Rev__74BC79AE404D7702");

            entity.ToTable("Task_Review");

            entity.Property(e => e.ReviewId).HasColumnName("ReviewID");
            entity.Property(e => e.ArchiveUrl).HasMaxLength(500);
            entity.Property(e => e.Comment).HasMaxLength(500);
            entity.Property(e => e.DocUrl).HasMaxLength(500);
            entity.Property(e => e.GitUrl).HasMaxLength(500);
            entity.Property(e => e.Pmid).HasColumnName("PMID");
            entity.Property(e => e.Result).HasDefaultValue((byte)1);
            entity.Property(e => e.ReviewTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TaskId).HasColumnName("TaskID");
            entity.Property(e => e.Version).HasDefaultValue(1);

            entity.HasOne(d => d.Pm).WithMany(p => p.TaskReviews)
                .HasForeignKey(d => d.Pmid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Review_User_PM");

            entity.HasOne(d => d.Task).WithMany(p => p.TaskReviews)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Review_Task");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CCAC2532B86A");

            entity.ToTable("User");

            entity.HasIndex(e => e.Username, "UQ__User__536C85E4A0BC5F34").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CreateTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.RealName).HasMaxLength(50);
            entity.Property(e => e.ResumeText).HasMaxLength(2000);
            entity.Property(e => e.Role).HasDefaultValue((byte)1);
            entity.Property(e => e.Skills).HasMaxLength(100);
            entity.Property(e => e.Status).HasDefaultValue((byte)1);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        modelBuilder.Entity<WorkLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__Work_Log__5E5499A821503AB8");

            entity.ToTable("Work_Log");

            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.LastTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status).HasDefaultValue((byte)1);
            entity.Property(e => e.TaskId).HasColumnName("TaskID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Task).WithMany(p => p.WorkLogs)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Log_Task");

            entity.HasOne(d => d.User).WithMany(p => p.WorkLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Log_User");
        });

        // 标签表配置
        modelBuilder.Entity<DictTag>(entity => {
            entity.HasKey(t => t.TagID);
            entity.ToTable("DictTags"); // 强制映射到数据库的单数表名
        });

        modelBuilder.Entity<TagRelation>(entity => {
            entity.HasKey(r => r.RelationID);
            entity.ToTable("TagRelation"); // 强制映射到数据库的单数表名
        });

        // 视图配置：无主键且映射到对应的 SQL View 名称
        modelBuilder.Entity<VProjectProgress>(e => { e.HasNoKey(); e.ToView("v_ProjectProgress"); });
        modelBuilder.Entity<VAuditWorkHours>(e => { e.HasNoKey(); e.ToView("v_AuditWorkHours"); });
        modelBuilder.Entity<VDevEfficiency>(e => { e.HasNoKey(); e.ToView("v_DevEfficiency"); });
        modelBuilder.Entity<VUserCapability>(e => { e.HasNoKey(); e.ToView("v_UserCapability"); });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
