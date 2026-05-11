<template>
  <div class="application-container">
    <el-card shadow="never">
      <el-table :data="applicationList" border stripe v-loading="loading">
        <el-table-column prop="ApplicationID" label="编号" width="90" align="center" />
        
        <el-table-column prop="TaskName" label="任务名称" width="200" align="center">
          <template #default="{ row }">
            <el-link type="primary" @click="router.push(`/task/detail/${row.TaskId}`)">
              {{ row.TaskName }}
            </el-link>
          </template>
        </el-table-column>

        <el-table-column v-if = "[1, 2, 4].includes(userRole)" prop="DevName" label="开发人员" align="center">
          <template #default="{ row }">
            <el-link type="success" @click="router.push(`/user/detail/${row.DevID}`)">
              {{ row.DevName }}
            </el-link>
          </template>
        </el-table-column>

        <el-table-column v-if = "[1, 4].includes(userRole)" prop="Type" label="类型" align="center">
          <template #default="{ row }">
            {{ row.Type === 1 ? 'PM邀请' : '开发人员申请' }}
          </template>
        </el-table-column>

        <el-table-column v-if = "[1, 2, 4].includes(userRole)" prop="DevSkills" label="开发人员技能" min-width="150" align="center">
          <template #default="{ row }">
            <template v-if="row.DevSkills">
              <el-tag 
                v-for="s in row.DevSkills.split(',')" 
                :key="s" 
                size="small" 
                class="mx-1"
              >{{ s }}</el-tag>
            </template>
            <span v-else>-</span>
          </template>
        </el-table-column>
        
        <el-table-column prop="ApplyTime" label="申请时间" min-width="160" align="center">
          <template #default="scope">
            <span>
              {{ scope.row.ApplyTime ? $dayjs(scope.row.ApplyTime).format('YYYY-MM-DD HH:mm') : '--' }}
            </span>
          </template>
        </el-table-column>

        <el-table-column label="状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="getStatusTagType(row.Status)">
              {{ getStatusLabel(row.Status) }}
            </el-tag>
          </template>
        </el-table-column>

        <el-table-column label="操作" width="120" fixed="right" align="center">
          <template #default="{ row }">
            <el-button 
              v-if="canAccept(row)"
              type="primary" 
              size="small" 
              @click="handleAccept(row.ApplicationID)"
            >
              接受
            </el-button>
            <span v-else>--</span>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, computed } from 'vue'; 
import { useRouter, useRoute } from 'vue-router';
import { ElMessage, ElMessageBox } from 'element-plus';
import { getTaskApplications, acceptApplication } from '../../api/task';
import { getCategories } from '../../api/common';
import type { TaskApplicationListDto } from '../../api/task';
import type { SelectOptionDto } from '../../api/common';

const router = useRouter();
const route = useRoute();
const userRole = computed(() => Number(localStorage.getItem('userRole') || 0));

const loading = ref(false);
const appStatusOptions = ref<SelectOptionDto[]>([]);
const applicationList = ref<TaskApplicationListDto[]>([]);
const pageStatus = computed(() => {
  const dir = route.query.direction;
  const role = userRole;

  if (dir === 'my' && role.value === 2) return 'pm-my';
  if (dir === 'you' && role.value === 2) return 'pm-you';
  if (dir === 'my' && role.value === 3) return 'dev-my';
  if (dir === 'you' && role.value === 3) return 'dev-you';
  return 'all';
});

// 监听Query的变化
watch(() => route.query.direction, () => {
  loadData();
}, { immediate: false });

// 数据加载
const loadData = async () => {
  loading.value = true;
  try {
    const direction = route.query.direction;
    let rawData;

    if (direction === 'my') {
      rawData = await getTaskApplications(1);
    } else if (direction === 'you') {
      rawData = await getTaskApplications(2);
    } else {
      rawData = await getTaskApplications();
    }
    applicationList.value = rawData as any;
  } catch (error) {
    ElMessage.error("数据加载失败");
  } finally {
    loading.value = false;
  }
};

const canAccept = (row: TaskApplicationListDto) => {
  if (row.Status !== 1) return false;
  
  if(pageStatus.value !== 'pm-you' && pageStatus.value !== 'dev-you'){
    return false;
  }
  return true; 
};

const initDictionary = async () => {
  const res = await getCategories('app-status') as any;
  appStatusOptions.value = res; 
};

const getStatusLabel = (statusValue: number) => {
  const option = appStatusOptions.value.find(opt => Number(opt.Value) === statusValue);
  return option ? option.Label : '...';
};

const getStatusTagType = (statusValue: number) => {
  const types: Record<number, string> = { 1: 'info', 2: 'success', 3: 'danger' };
  return types[statusValue] || '';
};

const handleAccept = async (appId: number) => {
  try {
    await ElMessageBox.confirm('确认执行此操作吗？', '提示');
    await acceptApplication(appId);
    ElMessage.success('操作成功');
    loadData(); 
  } catch (err) {}
};

onMounted(async () => {
  await initDictionary();
  loadData();
});
</script>

<style scoped>
.application-container {
  padding: 20px;
}
.mx-1 { margin: 0 4px; }
.mr-1 { margin-right: 4px; }
</style>