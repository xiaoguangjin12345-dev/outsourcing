import request from '../utils/request';
import type { ApiResponse, SelectOptionDto } from './common';

/**
 * 任务创建 DTO (TaskCreateDto)
 */
export interface TaskCreateDto {
  ProjectId: number;
  TaskName: string;
  TaskDescription?: string;
  RequiredSkills?: number[];
  EstimatedHours: number;
}

/**
 * 参数化查询对象 (TaskQueryDto)
 */
export interface TaskQueryDto {
  TaskName?: string;
  ProjectName?: string;
  PmName?: string;
  DevName?: string;
  ProjectIds?: number[];
  Statuses?: number[];
  Skills?: number[];
  ProjectId?: number;
}

/**
 * 任务列表返回对象 (TaskListDto)
 */
export interface TaskListDto {
  TaskId: number;
  TaskName: string;
  ProjectId: number;
  ProjectName: string;
  StatusName: string;
  EstimatedHours: number;
  RequiredSkills?: string; 
}

/**
 * 任务详情 (TaskDetailDto)
 */
export interface TaskDetailDto {
  TaskID: number;
  TaskName: string;
  TaskDescription?: string;
  RequiredSkills?: string;
  EstimatedHours: number;
  ActualHours?: number;
  StatusName: string;
  CreateTime: string;
  ProjectID: number;
  ProjectName: string;
  PMID: number;
  PMName: string;
  DevName?: string;
}

/**
 * 任务邀请/申请 (TaskInviteDto)
 */
export interface TaskInviteDto {
  DevID: number;
}

/**
 * 工时修改 (TaskHoursUpdateDto)
 */
export interface TaskHoursUpdateDto {
  NewEstimatedHours: number;
  ChangeReason: string;
}

/**
 * 任务意向申请列表项 (TaskApplicationListDto)
 */
export interface TaskApplicationListDto {
  ApplicationID: number;
  TaskId: number;
  TaskName: string;
  DevID: number;
  DevName: string;
  DevSkills?: string;
  Type: number;   // 1-PM邀请, 2-开发申请
  Status: number;
  ApplyTime: string;
}


// 创建任务 (POST /api/Task)
export const createTask = (data: TaskCreateDto) => {
  return request.post<ApiResponse<string>>('/Task', data);
};

// 参数化查询任务 (GET /api/Task)
export const getTasks = (query: TaskQueryDto) => {
  return request.get<ApiResponse<TaskListDto[]>>('/Task', { params: query });
};

// 查询任务具体详情 (GET /api/Task/{id})
export const getTaskById = (id: number) => {
  return request.get<ApiResponse<TaskDetailDto>>(`/Task/${id}`);
};

// Dev申请/PM邀请任务 (POST /api/Task/{id}/applications)
export const handleApplication = (id: number, data?: TaskInviteDto) => {
  return request.post<ApiResponse<string>>(`/Task/${id}/applications`, data);
};

// 同意任务申请/邀请 (PUT /api/Task/applications/{id})
export const acceptApplication = (id: number) => {
  return request.put<ApiResponse<string>>(`/Task/applications/${id}`);
};

// PM修改预估工时 (PUT /api/Task/{id}/hours)
export const updateTaskHours = (id: number, data: TaskHoursUpdateDto) => {
  return request.put<ApiResponse<string>>(`/Task/${id}/hours`, data);
};

// 查看申请/邀请列表 (GET /api/Task/applications)
// direction: 1-发出, 2-收到
export const getTaskApplications = (direction?: number) => {
  return request.get<ApiResponse<TaskApplicationListDto[]>>('/Task/applications', { 
    params: { direction } 
  });
};

// 任务广场：开发人员专用的待分配查询 (POST /api/Task/square)
export const getTaskSquare = (query: TaskQueryDto) => {
  return request.post<ApiResponse<TaskListDto[]>>('/Task/square', query);
};

// 获取任务下拉框 (用于工时填报) (GET /api/Task/options)
export const getTaskOptions = () => {
  return request.get<ApiResponse<SelectOptionDto[]>>('/Task/options');
};
