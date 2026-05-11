<template>
  <div style="width: 90%; margin: 20px auto;">
    <el-card shadow="never">
      <el-form :inline="true" :model="TaskQuery" class="demo-form-inline">
        <el-form-item label="任务名称">
          <el-input v-model="TaskQuery.TaskName" placeholder="搜索" clearable @keyup.enter="handleQuery" />
        </el-form-item>

        <el-form-item label="项目名称">
          <el-input v-model="TaskQuery.ProjectName" placeholder="搜索" clearable />
        </el-form-item>

        <el-form-item label="项目经理">
          <el-input v-model="TaskQuery.PmName" placeholder="搜索" clearable />
        </el-form-item>

        <el-form-item label="开发人员">
          <el-input v-model="TaskQuery.DevName" placeholder="搜索" clearable />
        </el-form-item>

        <el-form-item label="任务状态">
          <el-select 
            v-model="TaskQuery.Statuses" 
            multiple 
            collapse-tags 
            collapse-tags-tooltip
            :max-collapse-tags="5" 
            placeholder="多选" 
            style="width: 300px"
          >
            <el-option 
              v-for="item in statusOptions" 
              :key="item.Value" 
              :label="item.Label" 
              :value="Number(item.Value)" 
            />
          </el-select>
        </el-form-item>

        <el-form-item label="所属项目">
          <el-select 
            v-model="TaskQuery.ProjectIds" 
            multiple 
            collapse-tags 
            collapse-tags-tooltip
            :max-collapse-tags="5" 
            placeholder="多选" 
            style="width: 200px"
          >
            <el-option 
              v-for="item in projectOptions" 
              :key="item.Value" 
              :label="item.Label" 
              :value="Number(item.Value)" 
            />
          </el-select>
        </el-form-item>

        <el-form-item label="所需技能">
          <el-select 
            v-model="TaskQuery.Skills" 
            multiple 
            collapse-tags 
            collapse-tags-tooltip
            :max-collapse-tags="5" 
            placeholder="多选" 
            style="width: 200px"
          >
            <el-option 
              v-for="item in tagOptions" 
              :key="item.Value" 
              :label="item.Label" 
              :value="Number(item.Value)" 
            />
          </el-select>
        </el-form-item>

        <el-form-item>
          <el-button type="primary" @click="handleQuery">查询任务</el-button>
          <el-button @click="handleReset">重置查询条件</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <el-table 
      v-loading="loading" 
      :data="taskList" 
      border 
      stripe
      style="margin-top: 20px; width: 100%"
    >
      <el-table-column prop="TaskId" label="任务编号" width="80" align="center" />
      <el-table-column prop="TaskName" label="任务名称" min-width="120" show-overflow-tooltip align="center"/>
      <el-table-column v-if = "userRole !== 3" prop="ProjectName" label="所属项目" min-width="120" align="center"/>

      <el-table-column v-if = "userRole === 3" label="所属项目" width="120" align="center">
        <template #default="{ row }">
          <el-button 
            link 
            @click="router.push(`/project/detail/${row.ProjectId}`)"
          >
            {{ row.ProjectName }}
          </el-button>
        </template>
      </el-table-column>
      
      <el-table-column label="任务状态" width="120" align="center">
        <template #default="{ row }">
          <el-tag :type="statusTagMap(row.StatusName)">{{ row.StatusName }}</el-tag>
        </template>
      </el-table-column>

      <el-table-column v-if = "userRole !== 3" prop="EstimatedHours" label="预估工时" width="100" align="center"/>
      <el-table-column prop="RequiredSkills" label="所需技能" show-overflow-tooltip align="center"/>

      <el-table-column label="操作" width="200" fixed="right" align="center">
        <template #default="{ row }">
          <el-button link type="primary" @click="router.push(`/task/detail/${row.TaskId}`)">查看详情</el-button>

          <el-button 
            v-if="userRole === 2 && row.StatusName === '待分配'" 
            link 
            type="success" 
            @click="router.push({ 
              path: '/user/index', 
              query: { type: 'talent-pool', taskId: row.TaskId } 
            })"
          >
            邀请开发人员
          </el-button>

          <el-button 
            v-if="userRole === 2 && ['进行中', '待验收'].includes(row.StatusName)" 
            link 
            type="warning" 
            @click="taskHourChangeRef.open(row.TaskId, row.EstimatedHours)"
          >
            修改预估工时
          </el-button>

          <el-button 
            v-if="userRole === 3 && row.StatusName === '进行中'" 
            link 
            type="warning" 
            @click="router.push(`/task/submit/${row.TaskId}`)"
          >
            提交成果
          </el-button>
        </template>
      </el-table-column>
    </el-table>
    <TaskHourChangeDialog ref = "taskHourChangeRef" @refresh = handleReset() />
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { ElMessage } from 'element-plus';
import { getProjectOptions } from '../../api/project';
import { getTasks } from '../../api/task';
import { getCategories } from '../../api/common';
import type { TaskListDto, TaskQueryDto } from '../../api/task';
import type { SelectOptionDto } from '../../api/common';
import TaskHourChangeDialog from '../../components/task-hour-change.vue';


const router = useRouter();
const route = useRoute();

const userRole = Number(localStorage.getItem('userRole'));
const loading = ref(false);

const taskList = ref<TaskListDto[]>([]);
const statusOptions = ref<SelectOptionDto[]>([]);
const projectOptions = ref<SelectOptionDto[]>([]);
const tagOptions = ref<SelectOptionDto[]>([]);
const taskHourChangeRef = ref();

const initialQuery: TaskQueryDto = {
  TaskName: '',
  ProjectName: '',
  PmName: '',
  DevName: '',
  Statuses: [],
  ProjectIds: [],
  Skills: []
};
const TaskQuery = reactive<TaskQueryDto>({ ...initialQuery });

// 获取数据
const handleQuery = async () => {
  loading.value = true;
  try {
    const res = await getTasks(TaskQuery);
    taskList.value = (res as any) || [];
  } catch (error) {
    console.error('查询失败', error);
  } finally {
    loading.value = false;
  }
};

// 重置查询
const handleReset = () => {
  TaskQuery.TaskName = '';
  TaskQuery.ProjectName = '';
  TaskQuery.PmName = '';
  TaskQuery.DevName = '';
  TaskQuery.Statuses = [];
  TaskQuery.ProjectIds = [];
  TaskQuery.Skills = [];

  handleQuery();
};

const statusTagMap = (status: string) => {
  const map: Record<string, string> = {
    '待分配': 'info',
    '进行中': 'warning',
    '待验收': 'primary',
    '已完成': 'success',
  };
  return map[status] || '';
};

onMounted(async () => {
  try {
    const [statusRes, projectRes, tagRes] = await Promise.all([
      getCategories('task-status'),
      getProjectOptions(),
      getCategories('tags')
    ]);
    
    statusOptions.value = statusRes as any;
    projectOptions.value = projectRes as any;
    tagOptions.value = tagRes as any;

    handleQuery();
  } catch (err) {
    ElMessage.error('任务列表数据加载失败');
  }
});
</script>

<style scoped>
.demo-form-inline .el-form-item {
  margin-right: 16px;
  margin-bottom: 10px;
}
</style>