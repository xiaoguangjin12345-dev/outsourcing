<template>
  <el-dialog v-model="visible" title="审批操作" width="500px">
    
    <el-form :model="form">
      <el-form-item label="审批结果">
        <el-radio-group v-model="form.Result">
          <el-radio :label="true">通过</el-radio>
          <el-radio :label="false">驳回</el-radio>
        </el-radio-group>
      </el-form-item>

      <el-form-item label="审批意见">
        <el-input v-model="form.Reason" type="textarea" />
      </el-form-item>
    </el-form>

    <template #footer>
      <el-button @click="visible = false">取消</el-button>
      <el-button type="primary" @click="submit">确定</el-button>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue';
import { ElMessage } from 'element-plus';
import { approveProject, archiveProject } from '../api/project';
import { processReview } from '../api/review';
import { auditUser } from '../api/user';
import type { NoticeApproveDto } from '../api/notice';

const emit = defineEmits(['refresh']);

// 弹窗显示控制
const visible = ref(false);
// 记录当前项目 ID
const businessId = ref(0);
// actionType 增加 'user-audit'
const actionType = ref<'approve' | 'archive' | 'review' | 'user-audit'>('approve');

// 审批表单对象
const form = reactive<NoticeApproveDto>({
  Result: true,
  Reason: ''
});

// 增加 review 类型支持
const open = (id: number, type: 'approve' | 'archive' | 'review' | 'user-audit') => {
  businessId.value = id;
  actionType.value = type;
  form.Result = true;
  form.Reason = '';
  visible.value = true;
};

const submit = async () => {
  const actions = {
    'approve': approveProject,
    'archive': archiveProject,
    'review': processReview,
    'user-audit': auditUser
  };

  try {
    // 找到对应的接口函数并执行
    const targetAction = actions[actionType.value];
    if (targetAction) {
      await targetAction(businessId.value, form);
      
      ElMessage.success('处理完成');
      visible.value = false;
      emit('refresh');
    }
  } catch (error) {
    console.error('提交失败:', error);
  }
};

// 公开open方法，供父组件调用
defineExpose({ 
  open 
});

</script>