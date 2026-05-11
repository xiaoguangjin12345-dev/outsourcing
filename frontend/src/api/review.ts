import request from '../utils/request';
import type { ApiResponse } from './common';
import type { NoticeApproveDto } from './notice';

/**
 * Dev 提交成果 DTO (ReviewSubmitDto)
 * 涉及文件上传，前端需转为 FormData
 */
export interface ReviewSubmitDto {
  TaskId: number;
  GitUrl?: string;
  ArchiveFile?: File; // 对应后端 IFormFile
  DocFile?: File;     // 对应后端 IFormFile
}

/**
 * 评审列表 DTO (ReviewListDto)
 */
export interface ReviewListDto {
  ReviewId: number;
  TaskName: string;
  Version: number;
  ResultName?: string; 
  Result: number;     
  Comment?: string;
  PmName?: string;
  ReviewTime: string;
  GitUrl?: string;
  ArchiveUrl?: string;
  DocUrl?: string;
}



/**
 * Dev 提交成果，生成初始评审记录 (POST /api/Review)
 */
export const submitReview = (data: ReviewSubmitDto) => {
  const formData = new FormData();
  Object.keys(data).forEach(key => {
    const value = (data as any)[key];
    if (value !== undefined && value !== null) {
      formData.append(key, value);
    }
  });

  return request.post<ApiResponse<string>>('/Review', formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  });
};

/**
 * PM 评审处理 (PUT /api/Review/{id})
 */
export const processReview = (id: number, data: NoticeApproveDto) => {
  return request.put<ApiResponse<string>>(`/Review/${id}`, data);
};

/**
 * 全角色 查看任务评审列表 (GET /api/Review)
 */
export const getReviewHistory = () => {
  return request.get<ApiResponse<ReviewListDto[]>>('/Review');
};