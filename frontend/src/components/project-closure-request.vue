<template>
  <el-dialog v-model="visible" title="项目结项申请" width="500px" @close="handleClose">
    <el-form label-width="100px">
      <el-form-item label="结项报告" required>
        <el-upload
          ref="uploadRef"
          action="#" 
          :auto-upload="false"
          :limit="1"
          accept=".pdf,.doc,.docx"
          :on-change="handleFileChange"
          :on-exceed="handleExceed"
          :on-remove="() => selectedFile = null"
        >
          <template #trigger>
            <el-button type="primary">选择文件</el-button>
          </template>
          <template #tip>
            <div class="el-upload__tip">请上传PDF或Word格式的结项报告</div>
          </template>
        </el-upload>
      </el-form-item>
    </el-form>

    <template #footer>
      <el-button @click="visible = false">取消</el-button>
      <el-button 
        type="danger" 
        :loading="submitting"
        :disabled="!selectedFile" 
        @click="submit"
      >提交申请</el-button>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { ElMessage, type UploadInstance, type UploadFile } from 'element-plus';
import { applyProjectClosure } from '../api/project';

const emit = defineEmits(['refresh']);
const visible = ref(false);
const submitting = ref(false);
const projectId = ref(0);
const uploadRef = ref<UploadInstance>();
const selectedFile = ref<File | null>(null); 

const open = (id: number) => {
  projectId.value = id;
  selectedFile.value = null;
  visible.value = true;
};

const handleFileChange = (uploadFile: UploadFile) => {
  const suffix = uploadFile.name.split('.').pop()?.toLowerCase();
  const isAllowed = ['pdf', 'doc', 'docx'].includes(suffix || '');
  
  if (!isAllowed) {
    ElMessage.error('只能上传PDF或Word文件');
    uploadRef.value?.clearFiles();
    return;
  }
  
  selectedFile.value = uploadFile.raw as File;
};

const handleExceed = () => {
  ElMessage.warning('请先删除旧文件再上传新文件');
};

const handleClose = () => {
  uploadRef.value?.clearFiles();
  selectedFile.value = null;
};

const submit = async () => {
  if (!selectedFile.value) return;

  submitting.value = true;
  
  const formData = new FormData();
  formData.append('FinalReportFile', selectedFile.value);

  try {
    await applyProjectClosure(projectId.value, formData);
    ElMessage.success('结项申请已提交');
    visible.value = false;
    emit('refresh');
  } catch (error) {
    console.error(error);
    ElMessage.error('提交失败');
  } finally {
    submitting.value = false;
  }
};

defineExpose({ open });
</script>