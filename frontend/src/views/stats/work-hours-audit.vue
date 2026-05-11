<template>
  <div class="page-container" style="width: 96%; margin: 20px auto;">
    <el-card shadow="never" class="filter-card">
      <div class="header-flex">
        <div class="page-title">工时偏差统计</div>
        <div class="search-bar">
          <el-radio-group v-model="currentDimension" @change="fetchData" size="default">
            <el-radio-button v-for="opt in dimensionOptions" :key="opt.Value" :label="opt.Value">
              {{ opt.Label }}
            </el-radio-button>
          </el-radio-group>
          <el-button type="primary" @click="fetchData">刷新数据</el-button>
        </div>
      </div>
    </el-card>

    <el-row :gutter="25" style="margin-top: 25px;">
      <el-col :span="13">
        <el-card shadow="never" class="chart-card">
          <template #header>
            <div class="card-header">
              <span class="header-title">维度对比</span>
              <div class="header-extra">
                <span class="dot est"></span> 预估工时
                <span class="dot act"></span> 实际工时
              </div>
            </div>
          </template>
          <el-card body-style="height: 410px">
            <div ref="chartRef" style="height: 100%; width: 100%;"></div>
          </el-card>
        </el-card>
      </el-col>

      <el-col :span="11">
        <el-card shadow="never" body-style="height: 450px" class="list-card">
          <template #header>
            <div class="card-header">
              <span class="header-title">工时偏差列表</span>
              <el-tag type="danger" size="small" effect="dark">按照预警风险程度排序</el-tag>
            </div>
          </template>
          
          <el-table :data="sortedAuditList" border stripe height="450px" v-loading="loading">
            <el-table-column prop="Name" label="维度" width="140" align="center"/>
            <el-table-column label="实际工时/预估工时" width="220" align="center">
              <template #default="scope">
                <div class="variance-cell">
                  <div class="hours-text">
                    <span :class="{ 'text-danger': scope.row.TotalActual > scope.row.TotalEstimated }">
                      {{ scope.row.TotalActual }}h
                    </span> 
                    / {{ scope.row.TotalEstimated }}h
                  </div>
                  <el-progress 
                    :percentage="calculateProgress(scope.row)" 
                    :status="scope.row.VarianceRate > 20 ? 'exception' : 'success'"
                    :show-text="false"
                    :stroke-width="6"
                  />
                </div>
              </template>
            </el-table-column>
            <el-table-column label="偏差率" width="140" align="center">
              <template #default="scope">
                <b :class="scope.row.VarianceRate > 20 ? 'text-danger' : 'text-success'">
                  {{ Number(scope.row.VarianceRate).toFixed(2) }}%
                </b>
              </template>
            </el-table-column>
          </el-table>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import * as echarts from 'echarts';
import { getWorkHours, getAuditDimensions } from '../../api/stats';
import type { WorkHoursDto } from '../../api/stats';

const loading = ref(false);
const dimensionOptions = ref<any[]>([]);
const currentDimension = ref("1"); 
const auditList = ref<WorkHoursDto[]>([]);
const chartRef = ref<HTMLElement | null>(null);
let myChart: echarts.ECharts | null = null;

const sortedAuditList = computed(() => {
  return [...auditList.value].sort((a, b) => b.VarianceRate - a.VarianceRate);
});

const calculateProgress = (row: WorkHoursDto) => {
  if (row.TotalEstimated === 0) return 0;
  return Math.min(Math.round((row.TotalActual / row.TotalEstimated) * 100), 100);
};

const fetchData = async () => {
  loading.value = true;
  try {
    if (dimensionOptions.value.length === 0) {
      const dRes = await getAuditDimensions();
      dimensionOptions.value = dRes as any;
    }
    const res = await getWorkHours(currentDimension.value);
    auditList.value = res as any;
    renderChart();
  } finally {
    loading.value = false;
  }
};

const renderChart = () => {
  if (!chartRef.value) return;
  if (!myChart) myChart = echarts.init(chartRef.value);

  const option = {
    backgroundColor: 'transparent',
    tooltip: { 
      trigger: 'axis', 
      axisPointer: { type: 'cross', crossStyle: { color: '#999' } },
      backgroundColor: 'rgba(255, 255, 255, 0.9)',
      borderWidth: 1,
      borderColor: '#eee'
    },
    legend: { show: false },
    grid: { left: '2%', right: '3%', bottom: '5%', top: '8%', containLabel: true },
    xAxis: {
      type: 'category',
      data: auditList.value.map(i => i.Name),
      axisTick: { alignWithLabel: true },
      axisLabel: { color: '#909399', fontSize: 11, rotate: 25 }
    },
    yAxis: { 
      type: 'value', 
      splitLine: { lineStyle: { type: 'dashed', color: '#f0f0f0' } }
    },
    series: [
      {
        name: '预估工时',
        type: 'bar',
        data: auditList.value.map(i => i.TotalEstimated),
        itemStyle: { color: '#409EFF', borderRadius: [4, 4, 0, 0] },
        barMaxWidth: 25
      },
      {
        name: '实际工时',
        type: 'bar',
        data: auditList.value.map(i => i.TotalActual),
        itemStyle: { 
          color: (params: any) => {
             const data = auditList.value[params.dataIndex] as any;
             return data.TotalActual > data.TotalEstimated ? '#F56C6C' : '#67C23A';
          },
          borderRadius: [4, 4, 0, 0] 
        },
        barMaxWidth: 25
      }
    ]
  };
  myChart.setOption(option);
  myChart.resize();
};

onMounted(() => {
  fetchData();
  window.addEventListener('resize', () => myChart?.resize());
});
</script>

<style scoped>
.header-flex { display: flex; justify-content: space-between; align-items: center; }
.page-title { font-size: 18px; font-weight: bold; color: #303133; }
.search-bar { display: flex; gap: 15px; }

.card-header { display: flex; justify-content: space-between; align-items: center; }
.header-title { font-weight: bold; font-size: 15px; color: #606266; }
.header-extra { font-size: 12px; color: #909399; }

.dot { display: inline-block; width: 8px; height: 8px; border-radius: 50%; margin: 0 4px 0 10px; }
.dot.est { background: #409EFF; }
.dot.act { background: #F56C6C; }

.variance-cell { padding: 4px 0; }
.hours-text { font-size: 12px; margin-bottom: 4px; color: #909399; }
.text-danger { color: #F56C6C; font-weight: bold; }
.text-success { color: #67C23A; }

:deep(.el-card__header) { padding: 12px 20px; background-color: #fafafa; }
</style>