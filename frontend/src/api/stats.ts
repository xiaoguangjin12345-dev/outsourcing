import request from '../utils/request';
import type { ApiResponse, SelectOptionDto } from './common';

/**
 * 进度大盘 DTO (ProjectProgressDto)
 */
export interface ProjectProgressDto {
  ProjectId: number;
  ProjectName: string;
  ProjectStatus: number;
  TotalTasks: number;
  CompletedTasks: number;
  ProgressRate: number;      // decimal -> number
}

/**
 * 成本偏差 DTO (WorkHoursDto)
 */
export interface WorkHoursDto {
  Name: string;            // 维度名称
  TotalEstimated: number;  // 预估总工时
  TotalActual: number;     // 实际总工时
  Variance: number;        // 偏差值
  VarianceRate: number;    // 偏差率
}

/**
 * 个体能力画像 DTO (UserCapabilityDto)
 */
export interface UserCapabilityDto {
  TagName: string;         // 技术标签
  AvgQuality: number;      // 平均质量分
  AvgEfficiency: number;   // 平均效率分
  AvgTotal: number;        // 平均综合分
  TaskCount: number;       // 完成任务数
}

/**
 * 开发人员效能对标 (DevEfficiencyDto)
 */
export interface DevEfficiencyDto {
  UserId: number;
  RealName: string;
  FinishedTasks: number;       // 完工总数
  AvgPerformanceScore: number; // 平均质量分
  TotalWorkHours: number;      // 投入总工时
}


// 获取进度大盘 (GET /api/Statistic/project-progress)
export const getProjectProgress = (projectIds?: number[]) => {
  return request.get<ApiResponse<ProjectProgressDto[]>>('/Statistic/project-progress', {
    params: { projectIds },
    paramsSerializer: {
      indexes: null 
    }
  });
};

// 获取成本偏差数据 (GET /api/Statistic/work-hours)
// dimension: project, user, tag
export const getWorkHours = (dimension: string = 'project') => {
  return request.get<ApiResponse<WorkHoursDto[]>>('/Statistic/work-hours', {
    params: { dimension }
  });
};

// 获取个体能力画像 (GET /api/Statistic/user-capability/{userId})
export const getUserCapability = (userId: number) => {
  return request.get<ApiResponse<UserCapabilityDto[]>>(`/Statistic/user-capability/${userId}`);
};

// 获取开发人员效能列表 (GET /api/Statistic/efficiency)
export const getEfficiency = () => {
  return request.get<ApiResponse<DevEfficiencyDto[]>>('/Statistic/efficiency');
};

/**
 * [导出] 开发人员效能 Excel (GET /api/Statistic/export)
 */
export const exportEfficiencyExcel = () => {
  return request.get('/Statistic/export', {
    responseType: 'blob'
  });
};

// 获取统计分析维度选项 (GET /api/Statistic/dimensions/options)
export const getAuditDimensions = () => {
  return request.get<ApiResponse<SelectOptionDto[]>>('/Statistic/dimensions/options');
};