<template>
  <div style="width: 100%; padding: 0 20px;">
    <el-card shadow="never" style="margin-bottom: 15px; border: none; border-bottom: 1px solid #ebeef5;">
      <el-form :inline="true" size="default">
        <el-form-item label="指定的开发人员">
          <el-select 
            v-model="selectedUserId" 
            placeholder="选择开发人员" 
            filterable
            style="width: 220px"
            @change="fetchCapability"
          >
            <el-option 
              v-for="item in devOptions" 
              :key="item.Value" 
              :label="item.Label" 
              :value="Number(item.Value)" 
            />
          </el-select>
        </el-form-item>
        <el-button type="primary" @click="fetchCapability">刷新数据</el-button>
      </el-form>
    </el-card>

    <el-row :gutter="12">
      <el-col :span="12">
        <el-card shadow="hover">
          <template #header>
            <div class="card-header">
              <span style="font-weight: bold">综合能力统计</span>
            </div>
          </template>
          <div ref="radarRef" style="height: 450px; width: 100%;"></div>
        </el-card>
      </el-col>

      <el-col :span="12">
        <el-card shadow="hover">
          <template #header>
            <div class="card-header">
              <span style="font-weight: bold">基于技能标签的聚合统计</span>
            </div>
          </template>

          <el-table :data="capabilityList" border stripe height="450px" v-loading="loading" size="default">
            <el-table-column prop="TagName" label="技能标签" width="100" fixed />
            
            <el-table-column label="平均质量分" align="center">
              <template #default="{ row }">
                <span style="color: #409EFF; font-weight: bold; font-size: 16px;">{{ row.AvgQuality }}</span>
              </template>
            </el-table-column>

            <el-table-column label="平均开发效率" min-width="150">
              <template #default="{ row }">
                <el-progress 
                  :percentage="row.AvgEfficiency" 
                  :stroke-width="8"
                  :color="row.AvgEfficiency > 80 ? '#67C23A' : '#E6A23C'"
                />
              </template>
            </el-table-column>

            <el-table-column label="平均综合得分" width="120" align="center">
              <template #default="{ row }">
                <el-tag effect="dark" type="warning" style="font-weight: bold; font-size: 14px;">
                  {{ row.AvgTotal }}
                </el-tag>
              </template>
            </el-table-column>

            <el-table-column prop="TaskCount" label="已完成任务总数" width="80" align="center" />
          </el-table>
          
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import * as echarts from 'echarts';
import { getUserCapability } from '../../api/stats';
import type { UserCapabilityDto } from '../../api/stats';
import type { ApiResponse, SelectOptionDto } from '../../api/common';
import {getDeveloperOptions} from '../../api/user';

const loading = ref(false);
const selectedUserId = ref<number | undefined>(undefined);
const devOptions = ref<SelectOptionDto[]>([]);
const capabilityList = ref<UserCapabilityDto[]>([]);
const radarRef = ref<HTMLElement | null>(null);
let myChart: echarts.ECharts | null = null;

const fetchDevOptions = async () => {
  const res = await getDeveloperOptions();
  devOptions.value = res as any;
  if (devOptions.value.length > 0) {
    selectedUserId.value = Number(devOptions.value?.[0]?.Value);
    fetchCapability();
  }
};

const fetchCapability = async () => {
  if (!selectedUserId.value) return;
  loading.value = true;
  try {
    const res = await getUserCapability(selectedUserId.value);
    
    capabilityList.value = res as any; 
    
    if (capabilityList.value.length > 0) {
        renderRadar();
    }
  } catch (error) {
    console.error("开发人员能力画像数据加载失败", error);
  } finally {
    loading.value = false;
  }
};

const renderRadar = () => {
  if (!radarRef.value || !capabilityList.value || capabilityList.value.length === 0) {
    console.warn("尚未获取数据，暂不生成雷达图");
    return;
  }

  if (!myChart) {
    myChart = echarts.init(radarRef.value);
  }

  const indicator = capabilityList.value.map(item => ({
    name: item.TagName,
    max: 100
  }));

  const option = {
    color: ['#409EFF', '#67C23A', '#E6A23C'],
    tooltip: { trigger: 'item' },
    legend: {
      data: ['平均质量(Q)', '平均效率(E)', '综合得分(T)'],
      bottom: 0
    },
    radar: {
      indicator: indicator,
      radius: '65%',
      splitArea: {
        areaStyle: {
          color: ['#f8f9fa', '#fff'],
          shadowColor: 'rgba(0, 0, 0, 0.05)',
          shadowBlur: 10
        }
      },
      axisName: {
        color: '#333',
        backgroundColor: '#f5f7fa',
        borderRadius: 3,
        padding: [3, 5]
      }
    },
    series: [
      {
        type: 'radar',
        emphasis: { lineStyle: { width: 4 } },
        data: [
          {
            value: capabilityList.value.map(i => i.AvgQuality),
            name: '平均质量(Q)',
            symbolSize: 0,
            lineStyle: { type: 'dashed', width: 1 }
          },
          {
            value: capabilityList.value.map(i => i.AvgEfficiency),
            name: '平均效率(E)',
            symbolSize: 0,
            lineStyle: { type: 'dashed', width: 1 }
          },
          {
            value: capabilityList.value.map(i => i.AvgTotal),
            name: '综合得分(T)',
            areaStyle: { opacity: 0.4 },
            lineStyle: { width: 3 },
            symbolSize: 6
          }
        ]
      }
    ]
  };
  myChart.setOption(option);
};

onMounted(() => {
  fetchDevOptions();
  window.addEventListener('resize', () => myChart?.resize());
});
</script>

<style scoped>
.tag-group {
  display: flex;
  justify-content: space-between;
}
.el-tag {
  font-family: 'Consolas', monospace;
  font-weight: bold;
}
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
</style>