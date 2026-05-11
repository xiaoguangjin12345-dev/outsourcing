<template>
  <div style="width: 95%; margin: 20px auto;">
    <el-card style="margin-top: 20px;" shadow="never">
      <el-form :inline="true" :model="queryForm" class="demo-form-inline">
        <el-form-item label="任务名称">
          <el-input v-model="queryForm.TaskName" placeholder="关键字搜索" clearable />
        </el-form-item>
        <el-form-item label="操作 PM">
          <el-input v-model="queryForm.PmName" placeholder="姓名搜索" clearable />
        </el-form-item>
        <el-form-item label="修改日期">
          <el-date-picker
            v-model="dateRange"
            type="daterange"
            range-separator="至"
            start-placeholder="开始日期"
            end-placeholder="结束日期"
            value-format="YYYY-MM-DD"
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSearch">查询审计日志</el-button>
          <el-button @click="resetQuery">重置</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <el-table :data="changeLogs" border stripe style="margin-top: 20px" v-loading="loading">
      <el-table-column prop="ChangeId" label="日志ID" width="80" align="center" />
      <el-table-column prop="TaskName" label="关联任务" min-width="180">
        <template #default="{ row }">
          <span class="font-bold">{{ row.TaskName }}</span>
        </template>
      </el-table-column>
      <el-table-column prop="PmName" label="操作 PM" width="120" align="center">
        <template #default="{ row }">
          {{ row.PmName }}
        </template>
      </el-table-column>
      
      <el-table-column label="工时变动 (h)" width="220" align="center">
        <template #default="scope">
          <div class="change-wrapper">
            <span class="old-hours">{{ scope.row.OldHours }}h</span>
            <el-icon class="arrow-icon"><Right /></el-icon>
            <span :class="getChangeClass(scope.row.OldHours, scope.row.NewHours)">
              {{ scope.row.NewHours }}h
            </span>
            <span class="diff-tag" :class="getChangeClass(scope.row.OldHours, scope.row.NewHours)">
              ({{ (scope.row.NewHours - scope.row.OldHours) > 0 ? '+' : '' }}{{ scope.row.NewHours - scope.row.OldHours }}h)
            </span>
          </div>
        </template>
      </el-table-column>

      <el-table-column prop="ChangeReason" label="调整原因" show-overflow-tooltip />
      <el-table-column prop="ChangeTime" label="操作时间" min-width="160" align="center">
        <template #default="scope">
          <span>
            {{ scope.row.ChangeTime ? $dayjs(scope.row.ChangeTime).format('YYYY-MM-DD HH:mm') : '--' }}
          </span>
        </template>
      </el-table-column>


    </el-table>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, watch } from 'vue';
import { getTaskChangeLogs } from '../../api/log';
import type { TaskChangeLogListDto, TaskChangeLogQueryDto } from '../../api/log';
import { Right } from '@element-plus/icons-vue';

const changeLogs = ref<TaskChangeLogListDto[]>([]);
const dateRange = ref<string[]>([]);
const loading = ref(false);

const queryForm = reactive<TaskChangeLogQueryDto>({
  TaskName: '',
  PmName: '',
  StartDate: '',
  EndDate: ''
});

// 日期区间
watch(dateRange, (val) => {
  if (val && val.length === 2) {
    queryForm.StartDate = val[0];
    queryForm.EndDate = val[1];
  } else {
    queryForm.StartDate = '';
    queryForm.EndDate = '';
  }
});

// 查询审计日志
const handleSearch = async () => {
  loading.value = true;
  try {
    const res = await getTaskChangeLogs(queryForm);
    changeLogs.value = (res as any).Data || res;
  } catch (error) {
    console.error('获取审计日志失败', error);
  } finally {
    loading.value = false;
  }
};

const resetQuery = () => {
  queryForm.TaskName = '';
  queryForm.PmName = '';
  dateRange.value = [];
  handleSearch();
};


const getChangeClass = (oldH: number, newH: number) => {
  if (newH > oldH) return 'text-danger';
  if (newH < oldH) return 'text-success';
  return 'text-gray';
};

onMounted(() => {
  handleSearch();
});
</script>

<style scoped>
.change-wrapper {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
}
.old-hours {
  color: #909399;
  text-decoration: line-through;
}
.arrow-icon {
  color: #409EFF;
}
.diff-tag {
  font-size: 12px;
  margin-left: 4px;
}
.text-danger {
  color: #f56c6c;
  font-weight: bold;
}
.text-success {
  color: #67c23a;
  font-weight: bold;
}
.text-gray {
  color: #909399;
}
.font-bold {
  font-weight: bold;
}
</style>