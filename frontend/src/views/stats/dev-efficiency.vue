<template>
  <div class="page-container" style="width: 96%; margin: 20px auto;">
    <el-card shadow="never" class="filter-card">
      <div class="header-flex">
        <div class="page-title">开发人员效能对标</div>
        <div class="search-bar">
          <el-button type="success" :icon="Download" @click="handleExport" plain>导出效能报表 (.xlsx)</el-button>
          <el-button type="primary" icon="Refresh" @click="fetchData">刷新数据</el-button>
        </div>
      </div>
    </el-card>

    <el-row :gutter="25" style="margin-top: 25px;">
      <el-col :span="13">
        <el-card shadow="never">
          <template #header>
            <div class="card-header">
              <span class="header-title">效能分布：投入总工时(横轴) vs 平均质量分(纵轴)</span>
              <el-tag type="info" size="small">气泡大小 = 已完成的任务数量</el-tag>
            </div>
          </template>
          <div v-loading="loading">
            <div ref="bubbleRef" style="height: 450px; width: 100%;"></div>
          </div>
        </el-card>
      </el-col>

      <el-col :span="11">
        <el-card shadow="never">
          <template #header>
            <div class="card-header">
              <span class="header-title">开发人员效能综合榜单</span>
            </div>
          </template>
          <el-table :data="efficiencyList" border stripe height="450px" v-loading="loading">
            <el-table-column prop="RealName" label="姓名" width="120" fixed align="center"/>
            <el-table-column prop="FinishedTasks" label="完成任务数" sortable align="center" width="120" />
            <el-table-column label="平均质量分" sortable align="center" width="120" >
              <template #default="scope">
                <el-tag :type="scope.row.AvgPerformanceScore >= 85 ? 'success' : 'warning'" effect="light">
                  {{ scope.row.AvgPerformanceScore }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="TotalWorkHours" label="投入总工时" sortable align="center" width="120">
               <template #default="scope">{{ scope.row.TotalWorkHours }}h</template>
            </el-table-column>
          </el-table>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { Download, Refresh } from '@element-plus/icons-vue';
import { ElMessage } from 'element-plus';
import * as echarts from 'echarts';
import { getEfficiency, exportEfficiencyExcel } from '../../api/stats';
import type { DevEfficiencyDto } from '../../api/stats';

const loading = ref(false);
const efficiencyList = ref<DevEfficiencyDto[]>([]);
const bubbleRef = ref<HTMLElement | null>(null);
let myChart: echarts.ECharts | null = null;

const fetchData = async () => {
  loading.value = true;
  try {
    const res = await getEfficiency();
    efficiencyList.value = (res as any).data || res;
    renderChart();
  } finally {
    loading.value = false;
  }
};

const renderChart = () => {
  if (!bubbleRef.value) return;
  if (!myChart) myChart = echarts.init(bubbleRef.value);

  const option = {
    grid: { left: '3%', right: '15%', bottom: '5%', top: '10%', containLabel: true },
    tooltip: {
      backgroundColor: 'rgba(255, 255, 255, 0.95)',
      formatter: (param: any) => {
        return `
          <div style="padding:5px">
            <b style="font-size:14px; color:#409EFF">${param.data[3]}</b><br/>
            <hr style="border:0;border-top:1px solid #eee;margin:5px 0"/>
            投入总工时: <b>${param.data[0]}h</b><br/>
            平均质量分: <b>${param.data[1]}</b><br/>
            完成任务数: <b style="color:#67C23A">${param.data[2]}个</b>
          </div>
        `;
      }
    },
    xAxis: { 
      name: '投入总工时(h)', 
      splitLine: { lineStyle: { type: 'dashed' } },
      axisLine: { lineStyle: { color: '#999' } },
      nameTextStyle: {
        padding: [0, 0, 0, -7]
      }
    },
    yAxis: { 
      name: '平均质量分', 
      min: 60,
      splitLine: { lineStyle: { type: 'dashed' } },
      axisLine: { lineStyle: { color: '#999' } }
    },
    series: [{
      type: 'scatter',
      data: efficiencyList.value.map(i => [
        i.TotalWorkHours, 
        i.AvgPerformanceScore, 
        i.FinishedTasks, 
        i.RealName
      ]),
      symbolSize: (data: any) => Math.max(data[2] * 8, 15), 
      label: {
        show: true,
        formatter: (param: any) => param.data[3],
        position: 'top',
        color: '#666',
        fontSize: 10
      },
      itemStyle: {
        shadowBlur: 10,
        shadowColor: 'rgba(25, 183, 207, 0.3)',
        color: new echarts.graphic.RadialGradient(0.4, 0.3, 1, [
          { offset: 0, color: 'rgb(129, 227, 238)' },
          { offset: 1, color: 'rgb(25, 183, 207)' }
        ])
      }
    }]
  };
  myChart.setOption(option);
};

const handleExport = async () => {
  try {
    ElMessage.info('正在请求后端生成Excel...');
    const blob = await exportEfficiencyExcel();
    const url = window.URL.createObjectURL(new Blob([blob as any]));
    const link = document.createElement('a');
    link.href = url;
    const date = new Date().toLocaleDateString().replace(/\//g, '-');
    link.setAttribute('download', `开发人员效能_${date}.xlsx`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
    ElMessage.success('报表下载成功');
  } catch (error) {
    ElMessage.error('报表导出失败');
  }
};

onMounted(() => {
  fetchData();
  window.addEventListener('resize', () => myChart?.resize());
});
</script>

<style scoped>
.header-flex { display: flex; justify-content: space-between; align-items: center; }
.page-title { font-size: 18px; font-weight: bold; color: #303133; }
.search-bar { display: flex; gap: 12px; }

.card-header { display: flex; justify-content: space-between; align-items: center; }
.header-title { font-weight: bold; color: #606266; }

:deep(.el-card__header) { background-color: #fafafa; padding: 12px 20px; }
</style>