<template>
  <div style="width: 95%; margin: 20px auto;">
    <el-card style="margin-top: 20px;" shadow="never">
      <el-form :inline="true" :model="queryForm">
        <el-form-item label="任务名称">
          <el-input v-model="queryForm.TaskName" placeholder="关键字" clearable />
        </el-form-item>
        
        <el-form-item label="日期区间">
          <el-date-picker
            v-model="dateRange"
            type="daterange"
            range-separator="至"
            start-placeholder="开始日期"
            end-placeholder="结束日期"
            value-format="YYYY-MM-DD"
          />
        </el-form-item>

        <el-form-item v-if="userRole !== 1" label="开发人员姓名">
          <el-input v-model="queryForm.UserName" placeholder="搜索开发人员" clearable />
        </el-form-item>

        <el-button type="primary" @click="fetchLogs">查询</el-button>
        
        <el-button v-if="userRole === 3" type="success" @click="addRef.open()">填写报工</el-button>
      </el-form>
    </el-card>

    <el-table :data="logRecords" border stripe style="margin-top: 20px">
      <el-table-column prop="LogId" label="工时编号" width="90" align="center" />
      <el-table-column prop="WorkDate" label="工作日期" width="120" align="center" />
      <el-table-column prop="UserName" label="开发人员" width="120" align="center">
        <template #default="{ row }">
          {{ row.UserName }}
        </template>
      </el-table-column>

      <el-table-column prop="TaskName" label="关联任务" min-width="200" align="center">
        <template #default="{ row }">
          {{ row.TaskName }}
        </template>
      </el-table-column>

      <el-table-column prop="Hours" label="工时(h)" width="90" align="center">
        <template #default="{ row }">
          <b style="color: #409EFF;">{{ row.Hours }}</b>
        </template>
      </el-table-column>
      <el-table-column prop="Description" label="工作内容" width="250" align="center"/>
      
      <el-table-column label="状态" width="100" align="center">
        <template #default="scope">
          <el-tag :type="scope.row.Status === 1 ? 'success' : 'info'" size="small">
            {{ scope.row.Status === 1 ? '可修改' : '只读' }}
          </el-tag>
        </template>
      </el-table-column>

      <el-table-column prop="LastTime" label="最后修改时间" min-width="160" align="center">
        <template #default="scope">
          <span>
            {{ scope.row.LastTime ? $dayjs(scope.row.LastTime).format('YYYY-MM-DD HH:mm') : '--' }}
          </span>
        </template>
      </el-table-column>

      <el-table-column v-if="userRole === 3" label="操作" width="150" fixed="right" align="center">
        <template #default="scope">
          <template v-if="scope.row.Status === 1">
            <el-button size="small" link type="primary" @click="updateRef.open(scope.row)">修改</el-button>
            <el-button size="small" link type="danger" @click="handleDelete(scope.row.LogId)">删除</el-button>
          </template>
          <span v-else style="color: #999; font-size: 12px;">锁定</span>
        </template>
      </el-table-column>
    </el-table>

    <WorkLogAdd ref="addRef" @refresh="fetchLogs" />
    <WorkLogUpdate ref="updateRef" @refresh="fetchLogs" />
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, watch } from 'vue';
import { ElMessage, ElMessageBox } from 'element-plus';
import { getWorkLogs, deleteWorkLog } from '../../api/log';
import type { WorkLogRecord, WorkLogQueryDto } from '../../api/log';
import WorkLogAdd from '../../components/worklog-add.vue';
import WorkLogUpdate from '../../components/worklog-update.vue';

const userRole = Number(localStorage.getItem('userRole'));
const addRef = ref();
const updateRef = ref();
const logRecords = ref<WorkLogRecord[]>([]);
const dateRange = ref<string[]>([]);

const queryForm = reactive<WorkLogQueryDto>({
  TaskName: '',
  UserName: '',
  StartDate: '',
  EndDate: '',
  Statuses: []
});

// 监听日期变化，并同步到DTO
watch(dateRange, (val) => {
  if (val && val.length === 2) {
    queryForm.StartDate = val[0];
    queryForm.EndDate = val[1];
  } else {
    queryForm.StartDate = '';
    queryForm.EndDate = '';
  }
});

// 加载数据
const fetchLogs = async () => {
  try {
    const res = await getWorkLogs(queryForm);
    logRecords.value = res as any; 
  } catch (err) {
    console.error("加载工时失败", err);
  }
};

// 确认删除
const handleDelete = async (id: number) => {
  if (!id) {
    ElMessage.error("无效的记录ID");
    return;
  }
  
  try {
    await ElMessageBox.confirm(
      '确定要删除这条工时记录吗？', 
      '删除确认', 
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }
    );
    await deleteWorkLog(id);
    ElMessage.success('删除成功');
    fetchLogs();
  } catch (err) {
    // 用户取消或请求失败
  }
};

onMounted(() => {
  fetchLogs();
});
</script>

<style scoped>
.el-form--inline .el-form-item {
  margin-right: 15px;
}
</style>