import request from '../utils/request';
import type { ApiResponse, SelectOptionDto } from './common';
import type { NoticeApproveDto } from './notice';

/**
 * 用户基础信息 (UserDto)
 */
export interface UserDto {
  UserId: number;
  Username: string;
  RealName: string;
  Role: number;
  Email?: string;
  Phone?: string;
  Skills?: string;
}

/**
 * 用户查询条件 (UserQueryDto)
 */
export interface UserQueryDto {
  Roles?: number[];     
  Statuses?: number[];  
  RealName?: string;    
  Skills?: number[];    
}

/**
 * 用户详情 (UserDetailsDto)
 */
export interface UserDetailsDto {
  UserId: number;
  Username: string;
  RealName: string;
  Role: number;
  Email?: string;
  Phone?: string;
  ResumeText?: string;
  Skills?: string;      
}

/**
 * 个人信息修改 (UserProfileUpdateDto)
 */
export interface UserProfileUpdateDto {
  Email?: string;
  Phone?: string;
  ResumeText?: string;
  SkillTagIds?: number[]; 
}



// 多条件查找用户 (GET /api/User)
export const getUsers = (query: UserQueryDto) => {
  return request.get<ApiResponse<UserDto[]>>('/User', { params: query });
};

// 查单人详情 (GET /api/User/{id})
export const getUserById = (id: number) => {
  return request.get<ApiResponse<UserDetailsDto>>(`/User/${id}`);
};

// [系统管理员专用] 审批新用户 (PUT /api/User/{id}/audit)
export const auditUser = (id: number, data: NoticeApproveDto) => {
  return request.put<ApiResponse<string>>(`/User/${id}/audit`, data);
};

// 用户修改非关键信息 (PUT /api/User/profile)
export const updateProfile = (data: UserProfileUpdateDto) => {
  return request.put<ApiResponse<string>>('/User/profile', data);
};

// 获取项目经理下拉列表 (GET /api/User/pm/options)
export const getProjectManagerOptions = () => {
  return request.get<ApiResponse<SelectOptionDto[]>>('/User/pm/options');
};

// 获取开发人员下拉列表 (GET /api/User/dev/options)
export const getDeveloperOptions = () => {
  return request.get<ApiResponse<SelectOptionDto[]>>('/User/dev/options');
};