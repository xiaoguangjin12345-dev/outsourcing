<template>
  <div style="width: 80%; margin: 20px auto;">
    <el-page-header @back="router.back()" :title="isEdit ? '编辑项目' : '创建项目'" />

    <el-card style="margin-top: 20px;">
      <el-form :model="projectForm" label-width="120px">
        
        <el-form-item label="项目名称" required>
          <el-input v-model="projectForm.ProjectName" placeholder="请输入项目名称" />
        </el-form-item>

        <el-form-item label="客户名称">
          <el-input v-model="projectForm.ClientName" placeholder="客户或单位名称" />
        </el-form-item>

        <el-form-item label="客户电话">
          <el-input v-model="projectForm.ClientPhone" placeholder="客户电话" />
        </el-form-item>
        <el-form-item label="客户邮箱">
          <el-input v-model="projectForm.ClientEmail" placeholder="客户邮箱" />
        </el-form-item>

        <el-form-item label="项目描述">
          <el-input v-model="projectForm.ProjectDescription" type="textarea" :rows="4" placeholder="请简述该项目" />
        </el-form-item>


        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="项目预算">
              <el-input-number v-model="projectForm.Budget" :min="0" :precision="2" style="width: 100%" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="参与人数">
              <el-input-number v-model="projectForm.Personnel" :min="1" :step="1" style="width: 100%" />
            </el-form-item>
          </el-col>
        </el-row>

        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="开始日期" required>
              <el-date-picker v-model="projectForm.StartDate" type="date" value-format="YYYY-MM-DD" style="width: 100%" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="结束日期" required>
              <el-date-picker v-model="projectForm.EndDate" type="date" value-format="YYYY-MM-DD" style="width: 100%" />
            </el-form-item>
          </el-col>
        </el-row>

        <el-form-item label="需求文档">
          <el-upload
            ref="uploadRef"
            action="#"
            :auto-upload="false"
            :limit="1"
            :on-change="handleFileChange"
            :file-list="fileList"
          >
            <el-button type="primary">选择文件</el-button>
            <template #tip>
              <div class="el-upload__tip">
                <span v-if="isEdit && !projectForm.RequirementFile && oldFileUrl">
                  当前已有文件：<el-link :href="oldFileUrl" target="_blank" type="success">点击预览</el-link>
                </span>
                <span v-else>新文件将覆盖旧文件（支持 PDF/Word/Zip）</span>
              </div>
            </template>
          </el-upload>
        </el-form-item>



        <el-form-item>
          <el-button type="primary" @click="submitForm" :loading="submitting">保存提交</el-button>
          <el-button @click="router.back()">返回列表</el-button>
        </el-form-item>
        
      </el-form>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { ElMessage } from 'element-plus';
import { createProject, updateProject, getProjectById } from '../../api/project';
import type { ProjectCreateDto, ProjectDetailsDto } from '../../api/project';

const router = useRouter();
const route = useRoute();


const isEdit = computed(() => route.query.type === 'edit');
const submitting = ref(false);
const oldFileUrl = ref('');
const fileList = ref([]);

const projectForm = reactive<ProjectCreateDto>({
  ProjectName: '',
  ClientName: '',
  ClientPhone: '',
  ClientEmail: '',
  ProjectDescription: '',
  Budget: 0,
  Personnel: 1,
  StartDate: '',
  EndDate: '',
  RequirementFile: undefined
});

const handleFileChange = (uploadFile: any) => {
  projectForm.RequirementFile = uploadFile.raw;
};

onMounted(async () => {
  if (isEdit.value === true) {
    const id = Number(route.params.id);
    if (!id) return;
    
    try {
      const res = await getProjectById(id) as any;
      
      projectForm.ProjectName = res.ProjectName;
      projectForm.ClientName = res.ClientName;
      projectForm.ProjectDescription = res.ProjectDescription;
      projectForm.Budget = res.Budget;
      projectForm.Personnel = res.Personnel;
      projectForm.StartDate = res.StartDate;
      projectForm.EndDate = res.EndDate;
      
      oldFileUrl.value = res.RequirementDocUrl || '';
    } catch (error) {
      ElMessage.error('获取项目信息失败');
    }
  }
});

const submitForm = async () => {
  // 基本校验
  if (!projectForm.ProjectName || !projectForm.StartDate || !projectForm.EndDate) {
    ElMessage.warning('请补充必填项');
    return;
  }

  // 日期校验
  if (new Date(projectForm.StartDate) > new Date(projectForm.EndDate)) {
    ElMessage.error('开始日期不能晚于结束日期');
    return;
  }

  submitting.value = true;
  try {
    // 直接传递projectForm
    if (isEdit.value) {
      await updateProject(Number(route.params.id), projectForm);
      ElMessage.success('项目信息已更新，请等待立项审批');
    } else {
      await createProject(projectForm);
      ElMessage.success('项目创建成功，请等待立项审批');
    }
    router.push('/project/index?type=pending');
  } catch (error) {
    console.error('项目信息提交失败', error);
  } finally {
    submitting.value = false;
  }
};
</script>