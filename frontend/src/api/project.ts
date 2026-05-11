import request from '../utils/request';
import type { ApiResponse, SelectOptionDto } from './common'; 
import type { NoticeApproveDto } from './notice'; 

/**
 * 创建项目 DTO (ProjectCreateDto)
 */
export interface ProjectCreateDto {
  ProjectName: string;
  ClientName?: string;
  ClientEmail?: string;
  ClientPhone?: string;
  ProjectDescription?: string;
  Budget?: number;
  Personnel?: number;
  RequirementFile?: File; // 对应后端 IFormFile
  StartDate: string;   
  EndDate: string;
}

/**
 * 项目查询 DTO (ProjectQueryDto)
 */
export interface ProjectQueryDto {
  ProjectName?: string;
  Statuses?: number[];
  PMIDs?: number[];
}

/**
 * 项目列表项 (ProjectListDto)
 */
export interface ProjectListDto {
  ProjectId: number;
  ProjectName: string;
  ClientName?: string;
  ProjectDescription?: string;
  Budget?: number;
  Status: number;
  PmName: string;
  StartDate?: string;
  EndDate?: string;
  CreateTime: string;
}

/**
 * 项目详情 (ProjectDetailsDto)
 */
export interface ProjectDetailsDto extends ProjectListDto {
  ClientEmail?: string;
  ClientPhone?: string;
  Personnel?: number;
  RequirementDocUrl?: string;
  TaskCount: number;
  CompletedTaskCount: number;
}

/**
 * 结项申请 (ProjectClosureRequestDto)
 */
export interface ProjectClosureRequestDto {
  FinalReportFile: File; // 结项报告
}


// ---------------------------- 获取列表 & 详情 ----------------------------
/**
 * 修复获取列表函数
 */
export const getProjects = (query: ProjectQueryDto) => {
  return request.get('/Project', { 
    params: query,
    paramsSerializer: {
      indexes: null 
    }
  });
};

// 查看项目详情 (GET /api/Project/{id})
export const getProjectById = (id: number) => {
  return request.get<ApiResponse<ProjectDetailsDto>>(`/Project/${id}`);
};

// 获取项目下拉选项 (GET /api/Project/options)
export const getProjectOptions = () => {
  return request.get<ApiResponse<SelectOptionDto[]>>('/Project/options');
};

// ---------------------------- 创建 & 修改 (涉及文件 FromForm) ----------------------------

// 创建项目 (POST /api/Project)
// project.ts
export const createProject = (data: ProjectCreateDto) => {
  const formData = new FormData();

  formData.append('ProjectName', data.ProjectName ?? '');
  formData.append('ClientName', data.ClientName ?? '');
  formData.append('ClientPhone', data.ClientPhone ?? '');
  formData.append('ClientEmail', data.ClientEmail ?? '');
  formData.append('ProjectDescription', data.ProjectDescription ?? '');

  formData.append('Budget', (data.Budget ?? 0).toString());
  formData.append('Personnel', (data.Personnel ?? 1).toString());

  formData.append('StartDate', data.StartDate ?? '');
  formData.append('EndDate', data.EndDate ?? '');

  // 文件
  if (data.RequirementFile) {
    formData.append('RequirementFile', data.RequirementFile);
  }

  // axios自己识别FormData并自动生成带有boundary的header
  return request.post<ApiResponse<string>>('/Project', formData);
};

// 修改项目 (PUT /api/Project/{id})
export const updateProject = (id: number, data: ProjectCreateDto) => {
  const formData = new FormData();
  Object.keys(data).forEach(key => {
    const value = (data as any)[key];
    if (value !== undefined && value !== null) {
      formData.append(key, value);
    }
  });
  return request.put<ApiResponse<string>>(`/Project/${id}`, formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  });
};

// ---------------------------- 审批 & 状态变更 ----------------------------

// PMO 审批立项 (POST /api/Project/{id}/approve)
export const approveProject = (id: number, data: NoticeApproveDto) => {
  return request.post<ApiResponse<string>>(`/Project/${id}/approve`, data);
};

// 结项申请 (发送FormData)
export const applyProjectClosure = (id: number, data: FormData) => {
  return request.put<ApiResponse<string>>(`/Project/${id}/closure`, data, {
    headers: {
      'Content-Type': 'multipart/form-data'
    }
  });
};

// PMO 审批结项/归档 (POST /api/Project/{id}/archive)
export const archiveProject = (id: number, data: NoticeApproveDto) => {
  return request.post<ApiResponse<string>>(`/Project/${id}/archive`, data);
};


