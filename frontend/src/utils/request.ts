import axios from 'axios';
import { ElMessage } from 'element-plus';
import qs from 'qs';

const service = axios.create({
  baseURL: '/api', 
  timeout: 5000,
  paramsSerializer: (params) => {
    return qs.stringify(params, { arrayFormat: 'repeat' });
  }
});

service.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    
    if (token && token !== 'null' && token !== 'undefined') {
      config.headers['Authorization'] = `Bearer ${token}`;
      console.log('发送请求，携带Token:', token.substring(0, 15) + '...');
    } else {
      console.warn('检测到Token为空');
    }
    return config;
  },
  (error) => Promise.reject(error)
);

service.interceptors.response.use(
  (response) => {
    if (response.config.responseType === 'blob' || response.data instanceof Blob) {
      return response.data; 
    }

    const res = response.data;
    console.log('后端返回：', res);
    
    const code = res.code !== undefined ? res.code : res.Code;
    const message = res.message || res.Message || '业务逻辑异常';
    const data = res.data !== undefined ? res.data : res.Data;

    if (code !== 200) {
      ElMessage.error(message);
      return Promise.reject(new Error(message));
    }
    return data; 
  },
  (error) => {
    const status = error.response?.status;
    
    if (status === 401) {
      // 清除本地存储
      localStorage.removeItem('token');
      localStorage.removeItem('userRole');
      
      if (!window.location.pathname.includes('/login')) {
        ElMessage.error('登录失效，请重新登录');
        setTimeout(() => {
          window.location.href = '/login'; 
        }, 1000);
      }
    } else {
      const msg = error.response?.data?.message || '网络或系统异常';
      ElMessage.error(msg);
    }
    return Promise.reject(error);
  }
);

export default service;