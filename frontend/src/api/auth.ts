import request from '../utils/request';
import type { ApiResponse } from './common'; // 从common引入

/**
 * 登录请求 DTO
 * 属性名与 C# DTO 保持一致（首字母大写）
 */
export interface LoginRequestDto {
  Username: string;
  Password: string;
}

/**
 * 注册请求 DTO
 */
export interface RegisterRequestDto {
  Username: string;
  Password: string;
  Password2: string; 
  RealName: string;
  Role: number;
  Email?: string;
  Phone?: string;
}

/**
 * --- API 函数区 ---
 */

export const login = (data: any) => {
  return request.post('/Auth/login', data); 
};

export const register = (data: any) => {
  return request.post('/Auth/register', data);
};