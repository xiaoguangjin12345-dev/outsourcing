<template>
  <div class="page-container" style="width: 95%; max-width: 1100px; margin: 30px auto;">
    <el-page-header @back="handleBack" title="返回" />

    <el-card shadow="never" style="margin-top: 20px;" v-loading="loading">
      <el-form ref="formRef" :model="updateForm" label-position="top">
        <el-row :gutter="40">
          <el-col :span="12">
            <el-form-item label="电子邮箱">
              <el-input v-model="updateForm.Email" placeholder="请输入邮箱" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="联系电话">
              <el-input v-model="updateForm.Phone" placeholder="请输入手机号" />
            </el-form-item>
          </el-col>
        </el-row>

        <el-form-item label="技能标签">
          <el-select
            v-model="updateForm.SkillTagIds"
            multiple
            collapse-tags
            placeholder="请重新选择技能标签"
            style="width: 100%"
            :max-collapse-tags="8"
          >
            <el-option
              v-for="tag in tagOptions"
              :key="tag.Value"
              :label="tag.Label"
              :value="Number(tag.Value)"
            />
          </el-select>
        </el-form-item>

        <el-form-item label="个人简介">
          <el-input
            v-model="updateForm.ResumeText"
            type="textarea"
            :rows="12"
            placeholder="请详细描述您的个人简介"
            resize="none"
          />
        </el-form-item>

        <div class="form-footer">
          <el-button type="primary" size="large" @click="submitUpdate">保存更新</el-button>
          <el-button size="large" plain @click="handleBack">取消修改</el-button>
        </div>
      </el-form>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessage, ElMessageBox } from 'element-plus';
import { getUserById, updateProfile } from '../../api/user';
import { getCategories } from '../../api/common';
import type { UserProfileUpdateDto } from '../../api/user';
import type { SelectOptionDto } from '../../api/common';

const router = useRouter();
const loading = ref(false);
const tagOptions = ref<SelectOptionDto[]>([]);
const userId = Number(localStorage.getItem('userId'));

const updateForm = reactive<UserProfileUpdateDto>({
  Email: '',
  Phone: '',
  ResumeText: '',
  SkillTagIds: []
});

const initData = async () => {
  loading.value = true;
  try {
    const userRes = await getUserById(userId);
    const data = (userRes as any).data || userRes;
    
    updateForm.Email = data.Email;
    updateForm.Phone = data.Phone;
    updateForm.ResumeText = data.ResumeText;

    const tagRes = await getCategories('tags');
    tagOptions.value = (tagRes as any).data || tagRes;
  } finally {
    loading.value = false;
  }
};

const submitUpdate = async () => {
  try {
    await updateProfile(updateForm);
    ElMessage.success('个人资料已更新');
    router.push('/profile/index');
  } catch (err) {}
};

const handleBack = () => {
  ElMessageBox.confirm('确定取消当前修改吗？', '提示', { type: 'warning' })
    .then(() => router.push('/profile/index'))
    .catch(() => {});
};

onMounted(initData);
</script>

<style scoped>
.form-footer {
  margin-top: 30px;
  padding-top: 30px;
  border-top: 1px solid #f0f2f5;
  display: flex;
  justify-content: center;
  gap: 20px;
}
:deep(.el-form-item__label) {
  font-weight: bold;
  color: #303133;
}
</style>