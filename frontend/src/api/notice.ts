import request from '../utils/request';
import type { ApiResponse } from './common';

/**
 * 各业务审批结果通用模板 (NoticeApproveDto)
 */
export interface NoticeApproveDto {
  Result: boolean;   // true:通过, false:驳回
  Reason?: string;   
}

/**
 * 通知列表项 (NoticeDto)
 */
export interface NoticeDto {
  NoticeId: number;
  SenderName: string;
  Content: string;
  NoticeType: number;  
  Status: number;       // 1-未读, 2-已读
  CreateTime: string;   
}

/**
 * 通知查询条件 (NoticeQueryDto)
 */
export interface NoticeQueryDto {
  Statuses?: number[];      // 状态多选
  NoticeTypes?: number[];   // 类型多选
  SenderName?: string;      // 发送人模糊
  StartDate?: string;       // 起始日期
  EndDate?: string;         // 截止日期
}


// 获取收件箱列表 (GET /api/Notice)
export const getInbox = (query: NoticeQueryDto) => {
  return request.get<ApiResponse<NoticeDto[]>>('/Notice', { params: query });
};

// 获取详情并标记已读 (GET /api/Notice/{id})
export const getNoticeDetail = (id: number) => {
  return request.get<ApiResponse<NoticeDto>>(`/Notice/${id}`);
};

// 逻辑删除消息 (PUT /api/Notice/{id}/delete)
export const deleteNotice = (id: number) => {
  return request.put<ApiResponse<string>>(`/Notice/${id}/delete`);
};

// 获取未读消息总数 (GET /api/Notice/unread-count)
export const getUnreadCount = () => {
  return request.get<ApiResponse<number>>('/Notice/unread-count');
};