<template>
  <el-dialog 
    v-model="visible" 
    title="根据项目分配任务" 
    width="600px" 
    @close="handleClose"
    :close-on-click-modal="false"
  >

    <el-form 
      ref="formRef"
      :model="form" 
      :rules="rules" 
      label-width="100px" 
      v-loading="loading"
    >
      <el-form-item label="任务名称" prop="TaskName">
        <el-input v-model="form.TaskName" placeholder="例如：前端登录模块开发" />
      </el-form-item>

      <el-form-item label="预计工时" prop="EstimatedHours">
        <el-input-number v-model="form.EstimatedHours" :min="1" />
        <span style="margin-left: 10px">小时</span>
      </el-form-item>

      <el-form-item label="所需技能" prop="RequiredSkills">
        <el-select
          v-model="form.RequiredSkills"
          multiple
          placeholder="请选择技能要求"
          style="width: 100%"
        >
          <el-option
            v-for="item in skillOptions"
            :key="item.Value"
            :label="item.Label"
            :value="Number(item.Value)"
          />
        </el-select>
      </el-form-item>

      <el-form-item label="任务描述" prop="TaskDescription">
        <el-input v-model="form.TaskDescription" type="textarea" :rows="3" />
      </el-form-item>
    </el-form>

    <template #footer>
      <el-button @click="visible = false">取消</el-button>
      <el-button type="primary" :loading="submitting" @click="handleSubmit">确认分配</el-button>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue';
import { ElMessage, type FormInstance } from 'element-plus';
import { getCategories, type SelectOptionDto } from '@/api/common';
import { createTask } from '@/api/task';

const emit = defineEmits(['refresh']);

const visible = ref(false);
const loading = ref(false);
const submitting = ref(false);
const formRef = ref<FormInstance>();
const skillOptions = ref<SelectOptionDto[]>([]);


const form = reactive({
  ProjectId: 0,
  TaskName: '',
  TaskDescription: '',
  RequiredSkills: [] as number[],
  EstimatedHours: 8
});

const rules = {
  TaskName: [{ required: true, message: '请输入任务名称', trigger: 'blur' }],
  RequiredSkills: [{ required: true, message: '请选择所需技能', trigger: 'change' }]
};

const open = async (pId: number, pName: string, pDesc: string) => {
  form.ProjectId = pId;
  visible.value = true;
  
  if (skillOptions.value.length === 0) {
    await loadSkills();
  }
  console.log('当前接收到的项目ID:', pId)
};

const loadSkills = async () => {
  loading.value = true;
  try {
    const res = await getCategories('tags') as unknown as SelectOptionDto[];
    skillOptions.value = res || [];
  } catch (error) {
    console.error('获取技能字典失败:', error);
  } finally {
    loading.value = false;
  }
};

const handleSubmit = async () => {
  if (!formRef.value) return;
  await formRef.value.validate(async (valid) => {
    if (valid) {
      submitting.value = true;
      try {
        await createTask(form);
        ElMessage.success('任务创建成功');
        visible.value = false;
        emit('refresh');
      } finally {
        submitting.value = false;
      }
    }
  });
};


const handleClose = () => {
  formRef.value?.resetFields();
};

defineExpose({ open });
</script>

<style scoped>
.ai-results {
  background: #f0f7ff;
  border: 1px dashed #409eff;
  border-radius: 8px;
  padding: 12px;
  margin-bottom: 15px;
}
.ai-msg { font-size: 12px; color: #409eff; margin-bottom: 8px; font-weight: bold; }
.task-cards { display: flex; gap: 10px; overflow-x: auto; padding: 5px; }
.task-card-item {
  min-width: 160px; background: #fff; border: 1px solid #dcdfe6;
  border-radius: 4px; padding: 10px; cursor: pointer; position: relative;
}
.task-card-item:hover { border-color: #409eff; background: #ecf5ff; }
.t-name { font-weight: bold; font-size: 13px; display: block; }
.t-desc { font-size: 11px; color: #909399; margin-top: 4px; }
.card-mask {
  position: absolute; top: 0; left: 0; width: 100%; height: 100%;
  background: rgba(64, 158, 255, 0.8); color: #fff;
  display: flex; align-items: center; justify-content: center;
  opacity: 0; transition: 0.3s;
}
.task-card-item:hover .card-mask { opacity: 1; }
</style>