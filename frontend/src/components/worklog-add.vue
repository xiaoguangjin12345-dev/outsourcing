<template>
  <el-dialog v-model="visible" title="新增工时记录" width="500px" @closed="handleClosed">
    <el-form :model="form" label-width="100px" ref="formRef" :rules="rules">
      <el-form-item label="关联任务" prop="TaskId">
        <el-select v-model="form.TaskId" placeholder="选择你负责的任务" style="width: 100%">
          <el-option 
            v-for="item in taskOptions" 
            :key="item.Value" 
            :label="item.Label" 
            :value="Number(item.Value)" 
          />
        </el-select>
      </el-form-item>
      
      <el-form-item label="投入工时" prop="Hours">
        <el-input-number v-model="form.Hours" :precision="1" :step="1" :min="1" :max="24" />
        <span style="margin-left: 10px">小时</span>
      </el-form-item>
      
      <el-form-item label="工作日期" prop="WorkDate">
        <el-date-picker 
          v-model="form.WorkDate" 
          type="date" 
          placeholder="选择日期" 
          value-format="YYYY-MM-DD" 
          style="width: 100%" 
        />
      </el-form-item>
      
      <el-form-item label="工作内容" prop="Description">
        <el-input 
          v-model="form.Description" 
          type="textarea" 
          placeholder="简述工作内容" 
          maxlength="200"
          show-word-limit
        />
      </el-form-item>
    </el-form>
    
    <template #footer>
      <el-button @click="visible = false">取消</el-button>
      <el-button type="primary" :loading="submitting" @click="submit">确认添加</el-button>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue';
import { ElMessage } from 'element-plus';
import { postWorkLog } from '../api/log';
import { getTaskOptions } from '../api/task'; 
import type { WorkLogSubmitDto } from '../api/log';

const emit = defineEmits(['refresh']);
const visible = ref(false);
const submitting = ref(false);
const formRef = ref();
const taskOptions = ref<any[]>([]);


// 初始状态
const initialForm = {
  TaskId: undefined,
  Hours: 1,
  WorkDate: new Date().toLocaleDateString('en-CA'), // 获取YYYY-MM-DD格式
  Description: ''
};

const form = reactive<WorkLogSubmitDto>({ ...initialForm } as any);

// 校验规则
const rules = {
  TaskId: [{ required: true, message: '请选择关联任务', trigger: 'change' }],
  Hours: [{ required: true, message: '请填写工时', trigger: 'blur' }],
  WorkDate: [{ required: true, message: '请选择日期', trigger: 'change' }],
  Description: [{ required: true, message: '必须填写工作内容', trigger: 'blur' }]
};

// 打开弹窗
const open = async () => {
  visible.value = true;
  resetForm();
  
  try {
    const res = await getTaskOptions();
    taskOptions.value = res as any; 
  } catch (error) {
    console.error("获取任务列表失败", error);
  }
};

// 提交工时记录
const submit = async () => {
  if (!formRef.value) return;
  
  // 落实前端校验
  await formRef.value.validate(async (valid: boolean) => {
    if (!valid) return;
    
    submitting.value = true;
    try {
      await postWorkLog(form);
      ElMessage.success('工时日志提交成功');
      visible.value = false;
      emit('refresh');
    } catch (err) {
      // 错误拦截器自动处理
    } finally {
      submitting.value = false;
    }
  });
};

const resetForm = () => {
  Object.assign(form, initialForm);
  if (formRef.value) formRef.value.clearValidate();
};

const handleClosed = () => {
  resetForm();
};

defineExpose({ open });
</script>