<template>
  <el-dialog 
    v-model="visible" 
    title="消息详情" 
    width="500px" 
    destroy-on-close
    append-to-body
  >
    <div v-loading="loading" class="detail-container">
      <div class="meta-info">
        <span class="meta-item">发送人：<strong>{{ detail.SenderName }}</strong></span>
        <span class="meta-item">{{ detail.CreateTime? $dayjs(detail.CreateTime).format('YYYY-MM-DD HH:mm') : '--' }}</span>
      </div>
      
      <el-divider style="margin: 15px 0" />
      
      <div class="notice-content" :class="{ 'warning-content': detail.NoticeType === 4 }">
        <el-icon v-if="detail.NoticeType === 4" color="#F56C6C" style="margin-right: 5px;">
          <WarningFilled />
        </el-icon>
        {{ detail.Content }}
      </div>
    </div>

    <template #footer>
      <div class="dialog-footer">
        <el-button type="primary" @click="handleConfirm">我知道了</el-button>
      </div>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { getNoticeDetail } from '../api/notice';
import type { NoticeDto } from '../api/notice';
import { WarningFilled } from '@element-plus/icons-vue';

const visible = ref(false);
const loading = ref(false);
const detail = ref<NoticeDto>({} as NoticeDto);
const emit = defineEmits(['refresh']);

/**
 * 打开详情即视为“已读”
 */
const open = async (id: number) => {
  visible.value = true;
  loading.value = true;
  try {
    const res = await getNoticeDetail(id);
    detail.value = (res as any).Data || res.data || res;
    
    emit('refresh');
  } catch (err) {
    console.error("加载消息详情失败", err);
  } finally {
    loading.value = false;
  }
};

const handleConfirm = () => {
  visible.value = false;
};


const getTypeTag = (type: number) => {
  const map: Record<number, string> = {
    1: '',        // 系统-默认
    2: 'success', // 审核
    4: 'danger',  // 预警
  };
  return map[type] || 'info';
};

defineExpose({ open });
</script>

<style scoped>
.detail-container {
  min-height: 120px;
  padding: 0 10px;
}
.meta-info {
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 13px;
  color: #606266;
}
.meta-item {
  border-left: 1px solid #dcdfe6;
  padding-left: 12px;
}
.notice-content {
  line-height: 1.8;
  color: #303133;
  white-space: pre-wrap;
  word-break: break-all;
  padding: 10px 0;
  font-size: 15px;
}

.warning-content {
  color: #cf4444;
  background-color: #fff5f5;
  padding: 15px;
  border-radius: 4px;
}
.dialog-footer {
  text-align: right;
}
</style>