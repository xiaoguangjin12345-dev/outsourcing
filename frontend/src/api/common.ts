import request from '../utils/request';

/**
 * 后端 ApiResponse<T>
 */
export interface ApiResponse<T> {
  code: number;      // 状态码
  message: string;   // 提示消息
  data: T;           // 业务数据
}

/**
 * 后端 SelectOptionDto
 */
export interface SelectOptionDto {
  Value: string;     // 选项值
  Label: string;     // 显示文字
}

/**
 * 获取通用分类字典 (CommonSelectController)
 * @param type 字典类别 (user-role, project-status, tags 等)
 */
export const getCategories = (type: string) => {
  return request.get<ApiResponse<SelectOptionDto[]>>(`/common/${type}/options`);
};


/**
 * 通用文件下载接口 (FileController)
 * @param fileUrl 后端存储的相对路径或标识
 * 设置 responseType 为 'blob'
 */
export const downloadFile = (fileUrl: string) => {
  return request.get<any>('/File/download', {
    params: { fileUrl },
    responseType: 'blob'    // 二进制流
  });
};