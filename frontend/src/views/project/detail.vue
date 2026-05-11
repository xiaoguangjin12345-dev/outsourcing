<template>
  <div style="width: 90%; margin: 20px auto;" v-loading="loading">
    
    <el-page-header @back="router.back()" title="返回">
    </el-page-header>

    <el-card style="margin-top: 20px;">
      <el-descriptions title="基础信息" :column="3" border>
        <el-descriptions-item label="项目编号">{{ projectDetail.ProjectId }}</el-descriptions-item>
        <el-descriptions-item label="项目名称">{{ projectDetail.ProjectName }}</el-descriptions-item>
        <el-descriptions-item label="客户名称">{{ projectDetail.ClientName  || '无'}}</el-descriptions-item>
        <el-descriptions-item label="客户电话">{{ projectDetail.ClientPhone  || '无'}}</el-descriptions-item>
        <el-descriptions-item label="客户邮箱">{{ projectDetail.ClientEmail || '无' }}</el-descriptions-item>

        <el-descriptions-item label="项目经理">{{ projectDetail.PmName }}</el-descriptions-item>
        <el-descriptions-item label="项目预算">{{ projectDetail.Budget}}</el-descriptions-item>
        
        <el-descriptions-item label="开始时间"> {{ projectDetail.StartDate }} </el-descriptions-item>
        <el-descriptions-item label="结束时间"> {{ projectDetail.EndDate }}</el-descriptions-item>
        
        <el-descriptions-item label="需求文档">
          <el-link 
            v-if="projectDetail.RequirementDocUrl" 
            type="primary" 
            @click="handleDownload"
          >
            下载文档
          </el-link>
          <el-tag v-else type="info">无</el-tag>
        </el-descriptions-item>

        <el-descriptions-item label="项目描述" :span="3">
          <div class="description-text">{{ projectDetail.ProjectDescription || '无' }}</div>
        </el-descriptions-item>
      </el-descriptions>
    </el-card>

    <el-card v-if="[3, 4, 5].includes(Number(projectDetail.Status))" style="margin-top: 20px;" >
      <template #header>
        <div class="card-header">
          <span>该项目关联的任务 ({{ taskList.length }})</span>
        </div>
      </template>
      <el-table :data="taskList" border stripe>
        <el-table-column prop="TaskId" label="任务编号" width="70" />
        <el-table-column prop="TaskName" label="任务名称" min-width="150" />
        <el-table-column prop="AssigneeName" label="关联的开发人员" width="120" />
        <el-table-column label="操作" width="100" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" @click="router.push(`/task/detail/${row.TaskId}`)">任务详情</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, reactive } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { getProjectById } from '../../api/project';
import { getTasks } from '../../api/task'; 
import type { ProjectDetailsDto } from '../../api/project';
import { downloadFile } from '@/api/common';
import { saveAs } from '@/utils/file';

const router = useRouter();
const route = useRoute();
const userRole = Number(localStorage.getItem('userRole'));

const loading = ref(false);
const projectDetail = ref<Partial<ProjectDetailsDto>>({});
const taskList = ref([]);

const handleDownload = async () => {
  const fileUrl = projectDetail.value.RequirementDocUrl;
  if (!fileUrl) return;

  try {
    const res = await downloadFile(fileUrl);
    const name = fileUrl.split('/').pop() || '项目文档';
    saveAs(res as any, name);
    
  } catch (error) {
    console.error('下载异常:', error);
  }
};

const initData = async () => {
  const id = Number(route.params.id);
  loading.value = true;
  try {
    // await异步
    const [detail, tasks] = await Promise.all([
      getProjectById(id),
      getTasks({ ProjectIds: [id] })
    ]);

    projectDetail.value = detail as any;
    taskList.value = tasks as any;
  } catch (err) {
    console.error('项目详情获取失败', err);
  } finally {
    loading.value = false;
  }
};

onMounted(initData);

</script>

<style scoped>
.description-text {
  white-space: pre-wrap;
  color: #606266;
  line-height: 1.6;
}
</style>