<template>
  <div class="page-container" style="width: 95%; margin: 20px auto;">
    <el-card shadow="never" class="filter-card">
      <div class="header-flex">
        <div class="page-title">项目进度监控</div>
        <div class="search-bar">
          <el-select 
            v-model="selectedProjectIds"
            multiple       
            collapse-tags 
            placeholder="筛选指定项目" 
            clearable 
            style="width: 260px"
          >
            <el-option 
              v-for="item in projectOptions" 
              :key="item.Value" 
              :label="item.Label" 
              :value="Number(item.Value)" 
            />
          </el-select>
          <el-button type="primary" @click="fetchProgress">刷新数据</el-button>
        </div>
      </div>
    </el-card>

    <div v-loading="loading" class="dashboard-body">
      <el-row :gutter="25" v-if="progressData.length > 0">
        <el-col :span="8" v-for="item in progressData" :key="item.ProjectId">
          <el-card shadow="hover" class="stat-card">
            <div class="card-top">
              <span class="p-name">{{ item.ProjectName }}</span>
              <el-tag :type="getStatusType(item.ProjectStatus)" size="small" effect="plain">
                {{ translateStatus(item.ProjectStatus) }}
              </el-tag>
            </div>

            <div class="chart-box">
              <el-progress 
                type="dashboard" 
                :percentage="Number(item.ProgressRate)" 
                :color="progressColors"
                :stroke-width="12"
                :width="160"
              />
              <div class="percentage-label">完成进度</div>
            </div>

            <div class="data-grid">
              <div class="data-item">
                <div class="data-val">{{ item.TotalTasks }}</div>
                <div class="data-lab">任务总数</div>
              </div>
              <div class="data-divider"></div>
              <div class="data-item">
                <div class="data-val text-success">{{ item.CompletedTasks }}</div>
                <div class="data-lab">已完成任务数</div>
              </div>
            </div>
          </el-card>
        </el-col>
      </el-row>

      <el-empty v-else description="暂无项目进度数据" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { getProjectProgress } from '../../api/stats';
import request from '../../utils/request';
import type { ProjectProgressDto } from '../../api/stats';
import type { ApiResponse, SelectOptionDto } from '../../api/common';

const loading = ref(false);
const selectedProjectIds = ref<number[]>([]);
const projectOptions = ref<SelectOptionDto[]>([]);
const progressData = ref<ProjectProgressDto[]>([]);

const progressColors = [
  { color: '#f56c6c', percentage: 20 },
  { color: '#e6a23c', percentage: 60 },
  { color: '#409EFF', percentage: 80 },
  { color: '#67c23a', percentage: 100 },
];

const fetchOptions = async () => {
  const res = await request.get<ApiResponse<SelectOptionDto[]>>('/project/options');
  projectOptions.value = res as any;
};

const fetchProgress = async () => {
  loading.value = true;
  try {
    const res = await getProjectProgress(selectedProjectIds.value);
    console.log(selectedProjectIds.value)
    progressData.value = res as any;
  } finally {
    loading.value = false;
  }
};

const translateStatus = (status: number) => {
  const map: any = { 1: '待审核', 2: '待修改', 3: '进行中', 4: '待结项', 5: '已归档' };
  return map[status] || '未知';
};

const getStatusType = (status: number) => {
  const map: any = { 3: 'primary', 5: 'info', 4: 'warning' };
  return map[status] || 'info';
};

onMounted(async () => {
  await fetchOptions();
  fetchProgress();
});
</script>

<style scoped>
.header-flex {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.page-title {
  font-size: 18px;
  font-weight: bold;
  color: #303133;
}
.search-bar {
  display: flex;
  gap: 12px;
}

.dashboard-body {
  margin-top: 25px;
}

.stat-card {
  margin-bottom: 25px;
  border: 1px solid #ebeef5;
  transition: all 0.3s;
}
.stat-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 10px 20px rgba(0,0,0,0.05) !important;
}

.card-top {
  display: flex;
  justify-content: space-between;
  margin-bottom: 20px;
}
.p-name {
  font-weight: bold;
  color: #303133;
  font-size: 15px;
}

.chart-box {
  display: flex;
  flex-direction: column;
  align-items: center;
  position: relative;
  padding: 10px 0;
}
.percentage-label {
  position: absolute;
  bottom: 25px;
  font-size: 12px;
  color: #909399;
}

.data-grid {
  display: flex;
  justify-content: space-around;
  align-items: center;
  background: #fcfcfc;
  padding: 15px 0;
  border-radius: 8px;
  margin-top: 20px;
}
.data-item {
  text-align: center;
}
.data-val {
  font-size: 20px;
  font-weight: bold;
  color: #303133;
}
.data-lab {
  font-size: 12px;
  color: #909399;
  margin-top: 4px;
}
.data-divider {
  width: 1px;
  height: 30px;
  background: #eee;
}

.card-footer {
  margin-top: 20px;
  padding-top: 15px;
  border-top: 1px dashed #eee;
  display: flex;
  justify-content: space-between;
  font-size: 11px;
  color: #c0c4cc;
}
.rate-num {
  font-weight: bold;
  color: #909399;
}
.text-success { color: #67c23a; }
</style>