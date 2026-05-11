<template>
  <div class="page-container" style="width: 80%; margin: 20px auto;">
    <el-page-header @back="router.back()" title="返回" />

    <el-card shadow="never" style="margin-top: 20px;">
      <el-descriptions :column="2" border>
        <el-descriptions-item label="姓名">
          <span class="bold-text">{{ user.RealName }}</span>
        </el-descriptions-item>
        
        <el-descriptions-item label="角色">
          <el-tag :type="getRoleTag(user.Role)" size="small">
            {{ roleMap[user.Role] }}
          </el-tag>
        </el-descriptions-item>

        <el-descriptions-item label="用户名">
          {{ user.Username }}
        </el-descriptions-item>

        <el-descriptions-item label="联系电话">
          {{ user.Phone || '--' }}
        </el-descriptions-item>

        <el-descriptions-item label="电子邮箱" :span="2">
          {{ user.Email || '--' }}
        </el-descriptions-item>

        <el-descriptions-item label="技能标签" :span="2">
          <template v-if="user.Skills">
            <el-tag 
              v-for="skill in user.Skills.split(',')" 
              :key="skill" 
              style="margin-right: 8px"
              effect="plain"
              size="small"
            >
              {{ skill }}
            </el-tag>
          </template>
          <span v-else class="text-gray">无</span>
        </el-descriptions-item>

        <el-descriptions-item label="个人简介" :span="2">
          <div class="resume-box">
            {{ user.ResumeText || '无' }}
          </div>
        </el-descriptions-item>
        
      </el-descriptions>

      <div style="margin-top: 20px; text-align: right;">
        <el-button plain @click="router.back()">关闭页面</el-button>
      </div>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { getUserById } from '../../api/user';
import type { UserDetailsDto } from '../../api/user';

const route = useRoute();
const router = useRouter();
const userId = Number(route.params.id);

const user = ref<UserDetailsDto>({
  UserId: 0,
  Username: '',
  RealName: '',
  Role: 3
});

const roleMap: Record<number, string> = { 1: 'PMO', 2: '项目经理', 3: '开发人员', 4: '系统管理员' };

const getRoleTag = (role: number) => {
  const maps: any = { 1: 'warning', 2: 'primary', 3: 'success', 4: 'danger' };
  return maps[role] || 'info';
};

const loadUserData = async () => {
  try {
    const res = await getUserById(userId);
    user.value = (res as any).data || res;
  } catch (error) {
    console.error('Fetch user error:', error);
  }
};

onMounted(() => loadUserData());
</script>

<style scoped>
.bold-text {
  font-weight: bold;
  font-size: 15px;
  color: #303133;
}

.resume-box {
  min-height: 120px;
  line-height: 1.8;
  white-space: pre-wrap;
  color: #555;
  padding: 10px 0;
}

.text-gray {
  color: #909399;
  font-size: 13px;
}

.el-card {
  border: 1px solid #ebeef5;
}

:deep(.el-descriptions__label) {
  width: 120px;
  background-color: #fafafa !important;
  font-weight: 500;
}
</style>