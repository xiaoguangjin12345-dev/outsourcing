import request from '../utils/request';
import type { ApiResponse } from './common';

/**
 * 提交评分 DTO (PerformanceScoreDto)
 */
export interface PerformanceScoreDto {
  SubjectiveScore: number; // Metric3 (满意度 S 或 难度系数 M)
  Comment?: string;        // 评价意见
}

/**
 * 待评分绩效列表项 (PerformancePendingDto)
 */
export interface PerformancePendingDto {
  PerformanceId: number;
  PerformanceType: number;   // 1-项目, 2-任务
  ObjectName: string;        // 任务名或项目名
  BeEvalUserName: string;    // 被考核人姓名
  Metric1: number;           // Q(质量) 或 R(准时度)
  Metric2: number;           // E(效率) 或 审计扣分
}

/**
 * 已发布绩效视图 (PerformanceViewDto)
 */
export interface PerformanceViewDto {
  PerformanceId: number;
  PerformanceType: number;   // 1-项目, 2-任务
  ObjectName: string;
  BeEvalUserName: string;
  EvalUserName: string;
  Metric1: number;           // Q / R
  Metric2: number;           // E / 审计扣分
  Metric3: number;           // S / M
  TotalScore: number;        // 最终得分
  Comment?: string;
  EvaluateTime?: string;
}

/**
 * 绩效查询条件 (PerformanceQueryDto)
 */
export interface PerformanceQueryDto {
  PerformanceTypes?: number[]; // 类型多选
  ObjectName?: string;         // 关联名称模糊
  BeEvalUserName?: string;     // 被考核人姓名模糊
  StartDate?: string;          // 评价时间区间
  EndDate?: string;
}


/**
 * 提交评分并结算 (PUT /api/Performance/{id}/score)
 */
export const submitPerformanceScore = (id: number, data: PerformanceScoreDto) => {
  return request.put<ApiResponse<string>>(`/Performance/${id}/score`, data);
};

/**
 * 获取待评分绩效列表 (GET /api/Performance/pending)
 */
export const getPendingPerformances = () => {
  return request.get<ApiResponse<PerformancePendingDto[]>>('/Performance/pending');
};

/**
 * 查看已发布绩效列表 (GET /api/Performance)
 */
export const getReleasedPerformances = (query: PerformanceQueryDto) => {
  return request.get<ApiResponse<PerformanceViewDto[]>>('/Performance', { params: query });
};