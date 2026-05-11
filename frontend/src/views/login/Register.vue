<template>
  <div class="register-wrapper">
    <el-card class="register-card">
      <template #header>
        <div class="card-header">
          <h2>软件外包项目管理系统 - 注册</h2>
        </div>
      </template>

      <el-form :model="RegisterForm" label-width="90px" label-position="left">
        <el-form-item label="账号" required>
          <el-input v-model="RegisterForm.Username" placeholder="请输入登录账号" />
        </el-form-item>

        <el-form-item label="真实姓名" required>
          <el-input v-model="RegisterForm.RealName" placeholder="请输入真实姓名" />
        </el-form-item>

        <el-form-item label="密码" required>
          <el-input v-model="RegisterForm.Password" type="password" show-password placeholder="请输入密码" />
        </el-form-item>

        <el-form-item label="确认密码" required>
          <el-input v-model="RegisterForm.Password2" type="password" show-password placeholder="请再次输入密码" />
        </el-form-item>

        <el-form-item label="身份角色" required>
          <el-select v-model="RegisterForm.Role" placeholder="请选择您的角色" style="width: 100%">
            <el-option label="项目管理办公室 (PMO)" :value="1" />
            <el-option label="项目经理 (PM)" :value="2" />
            <el-option label="开发人员 (Dev)" :value="3" />
          </el-select>
        </el-form-item>

        <el-form-item label="电子邮箱">
          <el-input v-model="RegisterForm.Email" placeholder="example@domain.com" />
        </el-form-item>

        <el-form-item label="手机号码">
          <el-input v-model="RegisterForm.Phone" placeholder="手机号码" />
        </el-form-item>

        <div class="button-group">
          <el-button type="primary" @click="handleRegister" :loading="loading" class="full-width-btn">
            提交注册
          </el-button>
          <el-link type="info" @click="router.push('/login')" :underline="false">
            已有账号？返回登录
          </el-link>
        </div>
      </el-form>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessage } from 'element-plus';
import { register } from '../../api/auth';
import type { RegisterRequestDto } from '../../api/auth';

const router = useRouter();
const loading = ref(false);

const RegisterForm = reactive<RegisterRequestDto>({
  Username: '',
  Password: '',
  Password2: '',
  RealName: '',
  Role: 3,     // 默认Dev
  Email: '',
  Phone: ''
});

const handleRegister = async () => {
  if (!RegisterForm.Username.trim() || !RegisterForm.Password || !RegisterForm.RealName.trim()) {
    return ElMessage.warning('请补全必填信息');
  }
  
  if (RegisterForm.Password.length < 6) {
    return ElMessage.warning('密码至少需要6位');
  }

  if (RegisterForm.Password !== RegisterForm.Password2) {
    return ElMessage.error('两次输入的密码不一致');
  }

  loading.value = true;
  try {
    const submitData = {
      ...RegisterForm,
      Role: Number(RegisterForm.Role) 
    };

    console.log('提交给后端的数据：', submitData);
    
    await register(submitData);
    ElMessage.success('注册成功');
    router.push('/login'); 
  } catch (error: any) {
    console.error('详细错误：', error.response?.data);
  } finally {
    loading.value = false;
  }
};
</script>

<style scoped>
.register-wrapper {
  height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
}
.register-card {
  width: 450px;
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
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 15px;
  margin-top: 10px;
}
.full-width-btn {
  width: 100%;
  height: 40px;
  font-size: 16px;
}
</style>