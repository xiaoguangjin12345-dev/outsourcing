import request from '../utils/request';
import type { ApiResponse } from './common';

/**
 * Dev 记录工时 DTO (WorkLogSubmitDto)
 */
export interface WorkLogSubmitDto {
  TaskId: number;
  Hours: number;
  Description?: string;
  WorkDate: string; // DateTime 传字符串 "YYYY-MM-DD"
}

/**
 * Dev 修改已有日志 DTO (WorkLogUpdateDto)
 */
export interface WorkLogUpdateDto {
  Hours: number;      // 新的小时数
  Description?: string;
}

/**
 * 多维查询工时日志 (WorkLogQueryDto)
 */
export interface WorkLogQueryDto {
  TaskName?: string;  // 任务名模糊
  UserName?: string;  // 开发人员姓名模糊
  StartDate?: string; // 区间查询
  EndDate?: string;
  Statuses?: number[]; // 1-可修改, 2-只读
  TaskId?: number;
  UserId?: number;
}

/**
 * 工时日志展示对象 (对应后端 WorkLog Model)
 */
export interface WorkLogRecord {
  LogId: number;
  TaskId: number;
  TaskName: string;   
  UserId: number;
  UserName: string;   
  Hours: number;
  Description?: string;
  WorkDate: string;
  Status: number;     // 1-可修改, 2-只读
  LastTime: string;   
}

/**
 * PM 预估工时修改记录查询 (TaskChangeLogQueryDto)
 */
export interface TaskChangeLogQueryDto {
  TaskName?: string;
  PmName?: string;
  StartDate?: string;
  EndDate?: string;
}

/**
 * PM 预估工时修改记录展示 (TaskChangeLogListDto)
 */
export interface TaskChangeLogListDto {
  ChangeId: number;
  TaskName: string;
  PmName: string;
  OldHours: number;
  NewHours: number;
  ChangeReason: string;
  ChangeTime: string;
}


// ------------------------------- 工时管理 -------------------------------

// Dev 记录工时 (POST /api/WorkLog)
export const postWorkLog = (data: WorkLogSubmitDto) => {
  return request.post<ApiResponse<string>>('/WorkLog', data);
};

// Dev 修改工时 (PUT /api/WorkLog/{id})
export const updateWorkLog = (id: number, data: WorkLogUpdateDto) => {
  return request.put<ApiResponse<string>>(`/WorkLog/${id}`, data);
};

// Dev 删除工时 (DELETE /api/WorkLog/{id})
export const deleteWorkLog = (id: number) => {
  return request.delete<ApiResponse<string>>(`/WorkLog/${id}`);
};

// 多条件查询日志列表 (GET /api/WorkLog)
export const getWorkLogs = (query: WorkLogQueryDto) => {
  return request.get<ApiResponse<WorkLogRecord[]>>('/WorkLog', { params: query });
};

// ------------------------------- 审计日志查询 -------------------------------

// 查询 PM 预估工时修改记录 (GET /api/task-change)
export const getTaskChangeLogs = (query: TaskChangeLogQueryDto) => {
  return request.get<ApiResponse<TaskChangeLogListDto[]>>('/task-change', { params: query });
};