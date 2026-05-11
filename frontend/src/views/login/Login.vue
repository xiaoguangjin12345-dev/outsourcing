<template>
  <div class="login-wrapper">
    <el-card class="login-card">
      <template #header>
        <div class="card-header">
          <h2>软件外包项目管理系统 - 登录</h2>
        </div>
      </template>

      <el-form :model="LoginForm" label-width="50px" label-position="left">
        <el-form-item label="账号">
          <el-input 
            v-model="LoginForm.Username" 
            placeholder="请输入您的账号"
          />
        </el-form-item>

        <el-form-item label="密码">
          <el-input 
            v-model="LoginForm.Password" 
            type="password" 
            show-password 
            placeholder="请输入您的密码"
            @keyup.enter="handleLogin"
          />
        </el-form-item>

        <div class="button-group">
          <el-button 
            type="primary" 
            @click="handleLogin" 
            :loading="loading" 
            class="full-width-btn"
          >
            登录系统
          </el-button>
          
          <div class="footer-links">
            <el-link type="info" :underline="false">忘记密码？</el-link>
            <el-link type="primary" :underline="false" @click="router.push('/register')">
              新用户注册
            </el-link>
          </div>
        </div>
      </el-form>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessage } from 'element-plus';
import { login } from '../../api/auth';
import type { LoginRequestDto } from '../../api/auth';

const router = useRouter();
const loading = ref(false);

const LoginForm = reactive<LoginRequestDto>({
  Username: '',
  Password: ''
});

const handleLogin = async () => {
  if (!LoginForm.Username.trim() || !LoginForm.Password.trim()) {
    return ElMessage.warning('账号或密码不能为空');
  }

  loading.value = true;
  try {
    const res = await login(LoginForm) as any; 
    
    // 后端返回的LoginResponseDto
    if (res && (res.Token) ) { 
      localStorage.setItem('token', res.Token);
      localStorage.setItem('userRole', String(res.Role));
      localStorage.setItem('userName', res.RealName || LoginForm.Username);
      localStorage.setItem('userId', String(res.UserId));

      ElMessage.success(`欢迎回来，${res.RealName || '用户'}`);
      router.push('/');
    } else {
      ElMessage.error('登录异常：未获取到有效Token');
    }
  } catch (error: any) {
    console.error('登录异常:', error);
  } finally {
    loading.value = false;
  }
};
</script>

<style scoped>
.login-wrapper {
  height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
}

.login-card {
  width: 400px;
  border-radius: 12px;
  box-shadow: 0 8px 20px rgba(0,0,0,0.1);
}

.card-header h2 {
  text-align: center;
  margin: 0;
  color: #303133;
  font-size: 22px;
  letter-spacing: 1px;
}

.button-group {
  margin-top: 25px;
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.full-width-btn {
  width: 100%;
  height: 40px;
  font-size: 16px;
  font-weight: 500;
}

.footer-links {
  display: flex;
  justify-content: space-between;
  font-size: 13px;
}

:deep(.el-form-item__label) {
  font-weight: bold;
}
</style>