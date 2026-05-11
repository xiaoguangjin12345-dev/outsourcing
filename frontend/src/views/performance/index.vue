<template>
  <div class="page-container" style="width: 95%; margin: 20px auto;">
    <el-card shadow="never" style="margin-bottom: 20px;">
      <el-form :inline="true" :model="queryForm">
        <el-form-item :label="activeTab === 1 ? '项目名称' : '任务名称'">
          <el-input v-model="queryForm.ObjectName" placeholder="搜索" clearable />
        </el-form-item>
        
        <el-form-item v-if="[1, 4].includes(userRole) || (userRole === 2 && activeTab !== 1)" :label="activeTab === 1 ? '被考核项目经理' : '被考核开发人员'">
          <el-input v-model="queryForm.BeEvalUserName" placeholder="搜索" clearable />
        </el-form-item>

        <el-form-item label="日期区间">
          <el-date-picker
            v-model="dateRange"
            type="daterange"
            range-separator="至"
            start-placeholder="开始日期"
            end-placeholder="结束日期"
            value-format="YYYY-MM-DD"
            @change="handleDateChange"
          />
        </el-form-item>

        <el-button type="primary" @click="fetchData">查询</el-button>
      </el-form>
    </el-card>

    <el-tabs 
      v-model="activeTab" 
      type="border-card"
      @tab-change="handleTabChange"
    >
      <el-tab-pane v-if="[1, 2, 4].includes(userRole)" label="项目级绩效" :name="1" />
      <el-tab-pane label="任务级绩效" :name="2" />

      <el-table :data="performanceList" v-loading="loading" border stripe>
        <el-table-column :label="activeTab === 1 ? '项目名称' : '任务名称'" prop="ObjectName" min-width="150" align="center"/>
        <el-table-column v-if="[1, 4].includes(userRole) || (userRole === 2 && activeTab !== 1)" 
        :label="activeTab === 1 ? '被考核项目经理' : '被考核开发人员'" prop="BeEvalUserName" width="130" align="center"/>

        <el-table-column :label="activeTab === 1 ? '资源控制分 (R)' : '开发质量 (Q)'" width="150" align="center">
          <template #default="{ row }">
             {{ row.Metric1 }}
          </template>
        </el-table-column>

        <el-table-column :label="activeTab === 1 ? '预估工时审计扣分' : '效率分 (E)'" width="150" align="center">
          <template #default="{ row }">
            <span :style="{ color: activeTab === 1 ? '#F56C6C' : '#606266' }">
              {{ activeTab === 1 ? `-${row.Metric2}` : row.Metric2 }}
            </span>
          </template>
        </el-table-column>

        <el-table-column label="主观评分" prop="Metric3" width="110" align="center"/>
        <el-table-column v-if=" [1, 3, 4].includes(userRole) || (userRole === 2 && activeTab !== 2)" 
        :label="activeTab === 1 ? '评分PMO' : '评分项目经理'" prop="EvalUserName" width="120" align="center"/>

        <el-table-column label="最终得分" width="100" align="center">
          <template #default="{ row }">
            <b style="color: #f56c6c; font-size: 16px">{{ row.TotalScore }}</b>
          </template>
        </el-table-column>

        <el-table-column prop="Comment" label="考核评语" show-overflow-tooltip align="center"/>
        <el-table-column prop="EvaluateTime" label="发布时间" min-width="100" align="center">
          <template #default="scope">
            <span>
              {{ scope.row.EvaluateTime ? $dayjs(scope.row.EvaluateTime).format('YYYY-MM-DD HH:mm') : '--' }}
            </span>
          </template>
        </el-table-column>
      </el-table>
    </el-tabs>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue';
import { getReleasedPerformances } from '@/api/performance';
import type { PerformanceViewDto, PerformanceQueryDto } from '@/api/performance';

const userRole = Number(localStorage.getItem('userRole'));
const loading = ref(false);
const performanceList = ref<PerformanceViewDto[]>([]);
const dateRange = ref<string[]>([]);
const activeTab = ref(userRole === 3 ? 2 : 1); // Dev角色默认进任务页，其他角色进项目页

const queryForm = reactive<PerformanceQueryDto>({
  PerformanceTypes: [activeTab.value], 
  ObjectName: '',
  BeEvalUserName: '',
  StartDate: '',
  EndDate: ''
});

// 切换标签时，更新QueryDto并重新请求
const handleTabChange = (name: any) => {
  activeTab.value = name;
  queryForm.PerformanceTypes = [name];
  fetchData();
};

const handleDateChange = (val: string[] | null) => {
  if (val) {
    [queryForm.StartDate, queryForm.EndDate] = val;
  } else {
    queryForm.StartDate = queryForm.EndDate = '';
  }
};

const fetchData = async () => {
  loading.value = true;
  try {
    const res = await getReleasedPerformances(queryForm);
    performanceList.value = res as any; 
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  fetchData();
});
</script>