<template>
  <el-dialog v-model="visible" title="修改工时记录" width="500px" @closed="handleClosed">
    <el-form :model="form" label-width="100px" ref="formRef" :rules="rules">
      <el-form-item label="关联任务">
        <el-input :model-value="taskName" disabled />
      </el-form-item>

      <el-form-item label="投入工时" prop="Hours">
        <el-input-number v-model="form.Hours" :precision="1" :step="1" :min="1" :max="24" />
        <span style="margin-left: 10px">小时</span>
      </el-form-item>

      <el-form-item label="工作内容" prop="Description">
        <el-input 
          v-model="form.Description" 
          type="textarea" 
          placeholder="请填写修改后的工作内容" 
          maxlength="200"
          show-word-limit
          rows="3"
        />
      </el-form-item>
    </el-form>

    <template #footer>
      <el-button @click="visible = false">取消</el-button>
      <el-button type="primary" :loading="submitting" @click="submit">保存修改</el-button>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue';
import { ElMessage } from 'element-plus';
import { updateWorkLog } from '../api/log';
import type { WorkLogUpdateDto, WorkLogRecord } from '../api/log';

const emit = defineEmits(['refresh']);
const visible = ref(false);
const submitting = ref(false);
const formRef = ref();
const currentLogId = ref(0);
const taskName = ref('');

const open = (row: WorkLogRecord) => {
  currentLogId.value = row.LogId;
  taskName.value = row.TaskName; 
  
  form.Hours = row.Hours;
  form.Description = row.Description || '';
  visible.value = true;
};

const form = reactive<WorkLogUpdateDto>({
  Hours: 0,
  Description: ''
});

// 校验规则
const rules = {
  Hours: [{ required: true, message: '请填写工时', trigger: 'blur' }],
  Description: [{ required: true, message: '请描述修改后的内容', trigger: 'blur' }]
};


// 提交修改
const submit = async () => {
  if (!formRef.value) return;

  await formRef.value.validate(async (valid: boolean) => {
    if (!valid) return;

    submitting.value = true;
    try {
      await updateWorkLog(currentLogId.value, form);
      ElMessage.success('工时已更新');
      visible.value = false;
      emit('refresh');
    } catch (err) {
      console.error("修改失败", err);
    } finally {
      submitting.value = false;
    }
  });
};

const handleClosed = () => {
  if (formRef.value) formRef.value.clearValidate();
};

defineExpose({ open });
</script>