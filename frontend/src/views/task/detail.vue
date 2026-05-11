<template>
  <div class="page-container" style="width: 80%; margin: 20px auto;">
    <el-page-header @back="router.back()" title="返回" />

    <el-card shadow="never" style="margin-top: 20px;">
      <el-descriptions :column="2" border>
        <el-descriptions-item label="任务名称" :span="2">
          <span class="bold-text">{{ taskDetail.TaskName }}</span>
        </el-descriptions-item>
        
        <el-descriptions-item label="任务状态">
          <el-tag size="small">{{ taskDetail.StatusName }}</el-tag>
        </el-descriptions-item>

        <el-descriptions-item label="所属项目">
          <span class="text-link">{{ taskDetail.ProjectName }}</span>
        </el-descriptions-item>

        <el-descriptions-item label="项目经理">
          {{ taskDetail.PMName }}
        </el-descriptions-item>

        <el-descriptions-item label="开发人员">
          <span :class="taskDetail.DevName ? '' : 'text-gray'">
            {{ taskDetail.DevName || '待分配' }}
          </span>
        </el-descriptions-item>

        <el-descriptions-item v-if = "userRole !== 3" label="预估工时">
          {{ taskDetail.EstimatedHours }} h
        </el-descriptions-item>

        <el-descriptions-item label="实际工时">
          {{ taskDetail.ActualHours || 0 }} h
        </el-descriptions-item>

        <el-descriptions-item label="技能要求" :span="2">
          <el-tag 
            v-for="s in (taskDetail.RequiredSkills?.split(',') || [])" 
            :key="s" 
            size="small" 
            style="margin-right: 5px"
          >
            {{ s }}
          </el-tag>
          <span v-if="!taskDetail.RequiredSkills" class="text-gray">无</span>
        </el-descriptions-item>

        <el-descriptions-item label="任务描述" :span="2">
          <div class="desc-box">
            {{ taskDetail.TaskDescription || '无' }}
          </div>
        </el-descriptions-item>

        <el-descriptions-item label="创建时间" :span="2">
          {{ taskDetail.CreateTime? $dayjs(taskDetail.CreateTime).format('YYYY-MM-DD HH:mm') : '--' }}
        </el-descriptions-item>

      </el-descriptions>

    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { getTaskById } from '../../api/task';
import type { TaskDetailDto } from '../../api/task';

const route = useRoute();
const router = useRouter();

const userRole = Number(localStorage.getItem('userRole'));
const taskId = Number(route.params.id);
const taskDetail = ref<TaskDetailDto>({} as TaskDetailDto);

const loadTaskDetail = async () => {
  try {
    const res = await getTaskById(taskId);
    taskDetail.value = res as any;
  } catch (err) {
    console.error('任务详情加载失败');
  }
};

onMounted(loadTaskDetail);
</script>

<style scoped>
.bold-text {
  font-weight: bold;
  font-size: 15px;
  color: #303133;
}

.desc-box {
  min-height: 100px;
  line-height: 1.6;
  white-space: pre-wrap;
  color: #606266;
  padding: 8px 0;
}

.text-gray { color: #909399; }
.text-link { color: #409EFF; }

:deep(.el-card__header) {
  display: none;
}
</style>