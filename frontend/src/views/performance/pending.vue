<template>
  <div class="page-container" style="width: 95%; margin: 20px auto;">
    <el-tabs 
      v-model="activeType" 
      type="border-card"
    >
      <el-tab-pane v-if="[1, 4].includes(userRole)" label="项目级绩效" :name="1" />
      <el-tab-pane v-if="[2, 4].includes(userRole)" label="任务级绩效" :name="2" />

      <el-table :data="displayData" v-loading="loading" border stripe>
        <el-table-column :label="activeType === 1 ? '项目名称' : '任务名称'" min-width="300" align="center">
          <template #default="{ row }">
            {{ row.ObjectName }}
          </template>
        </el-table-column>

        <el-table-column prop="BeEvalUserName" :label="activeType === 1 ? '被考核项目经理' : '被考核开发人员'" min-width="200" align="center"/>

        <el-table-column :label="activeType === 1 ? '资源控制率' : '质量分'" min-width="180" align="center">
          <template #default="{ row }"> {{ row.Metric1 }} </template>
        </el-table-column>

        <el-table-column :label="activeType === 1 ? '行为审计扣分' : '工时效率分'" min-width="180" align="center">
          <template #default="{ row }">
            {{ activeType === 1 ? `-${row.Metric2}` : row.Metric2 }}
          </template>
        </el-table-column>

        <el-table-column v-if = "userRole !== 4" label="操作" min-width="200" align="center">
          <template #default="{ row }">
            <el-button type="primary" link @click="prefSubmitRef.open(row)">评分</el-button>
          </template>
        </el-table-column>
      </el-table>

    </el-tabs>

    <PrefSubmitDialog ref = "prefSubmitRef" @refresh="fetchData" />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import type { PerformancePendingDto } from '@/api/performance';
import { getPendingPerformances } from '@/api/performance';
import PrefSubmitDialog from '@/components/pref-submit.vue';

const userRole = Number(localStorage.getItem('userRole'));
const loading = ref(false);
const prefSubmitRef = ref();

// 原始数据
const allData = ref<PerformancePendingDto[]>([]);

// 当前选中的Tab类型 (1-项目, 2-任务)
const activeType = ref(1);


const displayData = computed(() => {
  // 如果是Admin，根据Tab选中的PerformanceType过滤
  if (userRole === 4) {
    return allData.value.filter(item => item.PerformanceType === activeType.value);
  }
  
  // 如果是PMO或PM，直接显示
  return allData.value;
});

const fetchData = async () => {
  loading.value = true;
  try {
    const res = await getPendingPerformances ();
    allData.value = res as any;
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  if(userRole === 1){
    activeType.value = 1;
  }
  else if(userRole === 2){
    activeType.value = 2;
  }
  fetchData();
});
</script>