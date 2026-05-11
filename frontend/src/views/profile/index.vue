<template>
  <div class="page-container" style="width: 85%; margin: 40px auto;">
    <el-card shadow="never" v-loading="loading">
      <el-descriptions :column="2" border>
        <el-descriptions-item label="姓名">
          <span class="bold-text">{{ userInfo.RealName }}</span>
        </el-descriptions-item>

        <el-descriptions-item label="用户名">
          {{ userInfo.Username }}
        </el-descriptions-item>

        <el-descriptions-item label="系统角色">
          <el-tag size="small" :type="getRoleType(userInfo.Role)">
            {{ translateRole(userInfo.Role) }}
          </el-tag>
        </el-descriptions-item>

        <el-descriptions-item label="联系电话">
          {{ userInfo.Phone || '未绑定' }}
        </el-descriptions-item>

        <el-descriptions-item label="电子邮箱">
          {{ userInfo.Email || '未绑定' }}
        </el-descriptions-item>

        <el-descriptions-item label="我的技能" :span="2">
          <div v-if="userInfo.Skills">
            <el-tag 
              v-for="s in userInfo.Skills.split(',')" 
              :key="s" 
              style="margin-right: 8px"
              effect="plain"
              size="small"
            >
              {{ s.trim() }}
            </el-tag>
          </div>
          <span v-else class="text-gray">暂未选择技能标签</span>
        </el-descriptions-item>

        <el-descriptions-item label="个人简介" :span="2">
          <div class="resume-box">
            {{ userInfo.ResumeText || '暂未填写' }}
          </div>
        </el-descriptions-item>
      </el-descriptions>

      <div class="profile-footer">
        <el-button 
          v-if="userRole !== 4" 
          type="primary" 
          @click="router.push('/profile/update')"
        >
          修改资料
        </el-button>
        
        <el-button 
          type="danger" 
          @click="handleLogout"
        >
          退出登录
        </el-button>
      </div>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessageBox, ElMessage } from 'element-plus';
import { getUserById } from '../../api/user';
import type { UserDetailsDto } from '../../api/user';

const router = useRouter();
const loading = ref(false);
const userInfo = ref<UserDetailsDto>({} as UserDetailsDto);

const userRole = Number(localStorage.getItem('userRole'));
const userId = Number(localStorage.getItem('userId'));

const fetchUserInfo = async () => {
  if (!userId) return;
  loading.value = true;
  try {
    const res = await getUserById(userId);
    userInfo.value = (res as any).data || res;
  } finally {
    loading.value = false;
  }
};

const handleLogout = () => {
  ElMessageBox.confirm('确定要退出系统吗？', '提示', {
    confirmButtonText: '退出',
    cancelButtonText: '取消',
    type: 'warning',
  }).then(() => {
    localStorage.clear(); 
    ElMessage.success('已退出登录');
    router.replace('/login'); 
  });
};

const translateRole = (role: number) => {
  const map: any = { 1: 'PMO', 2: '项目经理', 3: '开发人员', 4: '系统管理员' };
  return map[role] || '未知角色';
};

const getRoleType = (role: number) => {
  const map: any = { 1: 'warning', 2: 'primary', 3: 'success', 4: 'danger' };
  return map[role] || 'info';
};

onMounted(fetchUserInfo);
</script>

<style scoped>
.bold-text {
  font-weight: bold;
  color: #303133;
}

.text-gray {
  color: #909399;
  font-size: 13px;
}

.resume-box {
  min-height: 100px;
  line-height: 1.8;
  white-space: pre-wrap;
  color: #606266;
  padding: 10px 0;
}

.profile-footer {
  margin-top: 30px;
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

:deep(.el-descriptions__label) {
  width: 140px;
  background-color: #fafafa !important;
  font-weight: 500;
}
</style>