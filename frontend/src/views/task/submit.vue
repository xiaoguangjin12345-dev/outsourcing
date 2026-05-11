<template>
  <div style="width: 70%; margin: 20px auto;">
    <el-page-header @back="router.push('/task/index')" title="返回任务列表" content="提交任务成果" />

    <el-card style="margin-top: 20px;">
      <template #header>
        <div class="card-header">
          <span>任务提交</span>
        </div>
      </template>

      <el-form :model="submitForm" label-width="120px" label-position="top">
        
        <el-form-item label="Git仓库地址 (GitUrl)">
          <el-input v-model="submitForm.GitUrl" placeholder="https://github.com/..." clearable />
        </el-form-item>

        <el-row :gutter="40">
          <el-col :span="12">
            <el-form-item label="程序文件压缩包 (ArchiveFile)" required>
              <el-upload
                action="#"
                :auto-upload="false"
                :limit="1"
                :on-change="(file: any) => handleFileChange(file, 'ArchiveFile')"
              >
                <el-button type="primary">选择ZIP/RAR</el-button>
                <template #tip><div class="tip">源代码文件压缩包</div></template>
              </el-upload>
            </el-form-item>
          </el-col>

          <el-col :span="12">
            <el-form-item label="技术文档 (DocFile)">
              <el-upload
                action="#"
                :auto-upload="false"
                :limit="1"
                :on-change="(file: any) => handleFileChange(file, 'DocFile')"
              >
                <el-button type="success">选择PDF/DOC/DOCX</el-button>
                <template #tip><div class="tip">技术文档</div></template>
              </el-upload>
            </el-form-item>
          </el-col>
        </el-row>

        <el-form-item style="margin-top: 30px;">
          <el-button type="primary" size="large" @click="handleSubmit" :loading="submitting">
            提交任务成果
          </el-button>
          <el-button size="large" @click="router.back()">取消</el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ElMessage, ElMessageBox } from 'element-plus';
import { submitReview } from '../../api/review';
import type { ReviewSubmitDto } from '../../api/review';

const route = useRoute();
const router = useRouter();
const submitting = ref(false);

const submitForm = reactive<ReviewSubmitDto>({
  TaskId: Number(route.params.id), 
  GitUrl: '',
  ArchiveFile: undefined,
  DocFile: undefined
});


const handleFileChange = (file: any, field: 'ArchiveFile' | 'DocFile') => {
  if (file && file.raw) {
    submitForm[field] = file.raw;
  }
};


const handleSubmit = async () => {
  if (!submitForm.GitUrl && !submitForm.ArchiveFile && !submitForm.DocFile) {
    ElMessage.warning('请填写Git链接或提交文件');
    return;
  }

  try {
    await ElMessageBox.confirm('确定提交当前任务成果吗？', '提示', {
      confirmButtonText: '提交',
      cancelButtonText: '取消',
      type: 'info'
    });

    submitting.value = true;

    await submitReview(submitForm);

    ElMessage.success('任务成果已提交');
    router.push('/task/index');

  } catch (error) {
    if (error !== 'cancel') {
      console.error('任务成果提交失败:', error);
    }
  } finally {
    submitting.value = false;
  }
};
</script>

<style scoped>
.tip {
  font-size: 12px;
  color: #909399;
  margin-top: 5px;
  line-height: 1.4;
}

.card-header {
  font-size: 16px;
  font-weight: 600;
  color: #303133;
}

.el-upload {
  display: block;
  text-align: left;
}

.el-form-item {
  margin-bottom: 24px;
}
</style>