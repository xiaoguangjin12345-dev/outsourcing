using OutsourcingApplication.DTOs;
using OutsourcingApplication.DTOs.Common;

namespace OutsourcingApplication.Services.Interfaces
{
    public interface IProjectService
    {
        // 创建项目
        bool CreateProject(int PmId, ProjectCreateDto dto);
        // 获取项目列表
        List<ProjectListDto> GetProjects(int CurrentUserId, int CurrentRole, ProjectQueryDto dto);
        // 查看项目详情
        ProjectDetailsDto GetProjectDetails(int id, int currentRole, int currentUserId);
        // 修改项目信息
        string UpdateProject(int id, int currentUserId, ProjectCreateDto dto);
        // PMO审批立项
        bool ApproveProject(int projectId, int currentRole, int pmoId, NoticeApproveDto dto);
        // PM申请结项
        bool ApplyProjectClosure(int projectId, int pmId, ProjectClosureRequestDto dto);
        // PMO执行结项
        bool ApproveArchive(int projectId, int currentRole, int pmoId, NoticeApproveDto dto);
        // 获取项目 标签
        public List<SelectOptionDto> GetApprovedProjectOptions(int userId, int role);
    }
}
