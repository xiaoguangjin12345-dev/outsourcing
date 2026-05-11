<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { getCategories } from '@/api/common';
import { getProjects } from '@/api/project'; 
import { getProjectManagerOptions } from '@/api/user';
import type { ProjectListDto } from '@/api/project';
import type { SelectOptionDto } from '@/api/common';
import ApproveDialog from '@/components/approve.vue';
import ProjectClosureDialog from '@/components/project-closure-request.vue';
import TaskAssignDialog from '@/components/task-assign.vue';

const route = useRoute();
const router = useRouter();
const userRole = Number(localStorage.getItem('userRole'));

const list = ref<ProjectListDto[]>([]);
const loading = ref(false);
const approveRef = ref();
const projectClosureRef = ref();
const taskAssignRef = ref();

const pageStatus = ref('all');

const dict = reactive({
  statusOptions: [] as SelectOptionDto[],
  pmOptions: [] as SelectOptionDto[],
  statusMap: {} as Record<number, string>
});

const query = reactive({
  ProjectName: '',
  Statuses: [] as number[],
  PMIDs: [] as number[]
});

const doQuery = async () => {
  loading.value = true;

  const mode = route.query.type;

  if(mode === 'pending'){
    if(userRole === 1){
      pageStatus.value = 'pmo-pending';
      query.Statuses = [1];
    }
    else if(userRole === 2){
      pageStatus.value = 'pm-pending';
      query.Statuses = [1, 2];
    }
  }
  else if(mode === 'pass'){
    if (userRole === 1){
      pageStatus.value = 'pmo-pass';
    }
    else if (userRole === 2){
      pageStatus.value = 'pm-pass';
    }
    query.Statuses = [3, 4, 5];
  }
  else {
    pageStatus.value = 'all';
    query.Statuses = [];
  }

  try {
    const res = await getProjects(query) as unknown as ProjectListDto[];
    list.value = res || [];
  } catch (error) {
    console.error('项目列表查询失败:', error);
  } finally {
    loading.value = false;
  }
};

const filteredStatusOptions = computed(() => {
  const all = dict.statusOptions;
  
  if (pageStatus.value === 'pmo-pending') {
    return all.filter(opt => [1].includes(Number(opt.Value)));
  }
  else if (pageStatus.value === 'pm-pending') {
    return all.filter(opt => [1, 2].includes(Number(opt.Value)));
  }
  
  else if (pageStatus.value === 'pmo-pass' || pageStatus.value === 'pm-pass') {
    return all.filter(opt => [3, 4, 5].includes(Number(opt.Value)));
  }
  return all;
});

onMounted(async () => {
  const sRes = await getCategories('project-status') as unknown as SelectOptionDto[];
  dict.statusOptions = sRes || [];
  dict.statusOptions.forEach(i => {
    dict.statusMap[Number(i.Value)] = i.Label;
  });

  const pRes = await getProjectManagerOptions() as unknown as SelectOptionDto[];
  dict.pmOptions = pRes || [];

  // doQuery执行路由参数解析
  doQuery();
});
</script>

<template>
  <div class="page-container">
    <el-card>
      <el-form :inline="true" :model="query">
        <el-form-item label="项目名称">
          <el-input v-model="query.ProjectName" placeholder="查询" clearable />
        </el-form-item>

        <el-form-item label="项目状态">
          <el-select
            v-model="query.Statuses"
            multiple
            collapse-tags
            collapse-tags-tooltip
            :max-collapse-tags="5" 
            placeholder="全部状态"
            style="width: 200px" 
          >
            <el-option
              v-for="item in filteredStatusOptions"
              :key="item.Value"
              :label="item.Label"
              :value="Number(item.Value)"
            />
          </el-select>
        </el-form-item>

        <el-form-item label="项目经理">
          <el-select
            v-model="query.PMIDs"
            multiple
            collapse-tags
            collapse-tags-tooltip
            :max-collapse-tags="5" 
            placeholder="全部项目经理"
            style="width: 200px" 
          >
            <el-option
              v-for="item in dict.pmOptions"
              :key="item.Value"
              :label="item.Label"
              :value="Number(item.Value)"
            />
          </el-select>
        </el-form-item>

        <el-form-item>
          <el-button type="primary" @click="doQuery">查询</el-button>
        </el-form-item>
        <el-form-item>
          <el-button 
            v-if="pageStatus === 'pm-pending'" 
            type="success" 
            @click="router.push(`/project/edit?type=add`)"
          >
            新建项目
          </el-button>
        </el-form-item>

      </el-form>
    </el-card>

    <el-table :data="list" border v-loading="loading" style="margin-top: 20px">
      <el-table-column prop="ProjectId" label="项目编号" width="90" align="center"/>
      <el-table-column prop="ProjectName" label="项目名称" width="110" align="center"/>
      <el-table-column prop="ClientName" label="客户名称" width="110" align="center"/>
      <el-table-column prop="ProjectDescription" label="项目描述" width="250" align="center"/>
      <el-table-column prop="Budget" label="项目预算" width="90" align="center"/>
      <el-table-column label="状态" width="90" align="center">
        <template #default="{ row }">
          {{ dict.statusMap[row.Status] || row.Status }}
        </template>
      </el-table-column>
      <el-table-column prop="PmName" label="项目经理" width="110" align="center"/>
      <el-table-column prop="StartDate" label="开始时间" width="120" align="center"/>
      <el-table-column prop="EndDate" label="结束时间" width="120" align="center"/>
      <el-table-column prop="CreateTime" label="创建时间" width="150" align="center">
        <template #default="scope">
          <span>
            {{ scope.row.CreateTime ? $dayjs(scope.row.CreateTime).format('YYYY-MM-DD HH:mm') : '--' }}
          </span>
        </template>
      </el-table-column>
      

      <el-table-column label="操作" width="120" fixed="right">
        <template #default="{ row }">
          <el-button link type="primary" @click="router.push(`/project/detail/${row.ProjectId}`)">详情</el-button>

          <el-button 
            v-if="pageStatus === 'pmo-pending'" 
            link type="success" 
            @click="approveRef.open(row.ProjectId, 'approve')"
          >审批立项</el-button>

          <el-button 
            v-if="pageStatus === 'pm-pending'" 
            link type="warning" 
            @click="router.push(`/project/edit/${row.ProjectId}?type=edit`)"
          >修改</el-button>

          <el-button 
            v-if="pageStatus === 'pm-pass' && row.Status === 3" 
            link type="warning" 
            @click="taskAssignRef.open(row.ProjectId, row.ProjectName, row.ProjectDescription)"
          >分配任务</el-button>

          <el-button 
            v-if="pageStatus === 'pm-pass' && row.Status === 3" 
            link type="danger" 
            @click="projectClosureRef.open(row.ProjectId)"
          >申请结项</el-button>

          <el-button 
            v-if="pageStatus === 'pmo-pass' && row.Status === 4" 
            link type="success" 
            @click="approveRef.open(row.ProjectId, 'archive')"
          >审批结项</el-button>
        </template>
      </el-table-column>

    </el-table>

    <ApproveDialog ref = "approveRef" @refresh="doQuery" />
    <ProjectClosureDialog ref = "projectClosureRef" @refresh="doQuery" />
    <TaskAssignDialog ref = "taskAssignRef" @refresh="doQuery" />

  </div>
</template>