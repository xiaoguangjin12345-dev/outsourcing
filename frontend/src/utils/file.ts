import { ElMessage } from 'element-plus';

// 文件下载
export const saveAs = (data: BlobPart, fileName: string) => {
  if (!data) {
    ElMessage.error('文件数据为空，无法下载');
    return;
  }

  try {
    // 构造Blob对象
    const blob = new Blob([data]);
    
    // 创建临时的浏览器内存URL
    const blobUrl = window.URL.createObjectURL(blob);
    
    // 动态创建隐藏的a标签
    const link = document.createElement('a');
    link.style.display = 'none';
    link.href = blobUrl;
    link.download = fileName; 
    
    document.body.appendChild(link);
    link.click();
    
    // 释放内存
    document.body.removeChild(link);
    window.URL.revokeObjectURL(blobUrl);
    
    ElMessage.success('已发起下载');
  } catch (error) {
    console.error('下载失败', error);
    ElMessage.error('下载失败');
  }
};