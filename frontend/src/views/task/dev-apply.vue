<template>
  <div style="width: 90%; margin: 20px auto;">

    <el-card style="margin-top: 20px;">
      <el-form :inline="true" :model="QueryForm">
        <el-form-item label="任务名称">
          <el-input v-model="QueryForm.TaskName" placeholder="搜索" clearable />
        </el-form-item>
        <el-form-item label="项目名称">
          <el-input v-model="QueryForm.ProjectName" placeholder="搜索" clearable />
        </el-form-item>
        <el-form-item label="技能要求">
          <el-select v-model="QueryForm.Skills" multiple placeholder="按技能标签筛选" style="width: 240px">
            <el-option v-for="item in skillOptions" :key="item.Value" :label="item.Label" :value="Number(item.Value)" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSearch">查询任务</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <el-table :data="squareTasks" border stripe style="margin-top: 20px">
      <el-table-column prop="TaskId" label="编号" width="80" />
      <el-table-column prop="TaskName" label="任务名称" />
      <el-table-column prop="ProjectName" label="项目名称" />
      <el-table-column prop="RequiredSkills" label="技能要求" show-overflow-tooltip />
      
      <el-table-column label="操作" width="200" fixed="right">
        <template #default="scope">
          <el-button size="small" @click="goDetail(scope.row.TaskId)">查看详情</el-button>
          
          <el-button 
            size="small" 
            type="success" 
            @click="handleApply(scope.row.TaskId)"
          >
            申请该任务
          </el-button>
        </template>
      </el-table-column>
    </el-table>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessage, ElMessageBox } from 'element-plus';
import { getTaskSquare, handleApplication } from '../../api/task';
import { getCategories } from '../../api/common';
import type { TaskListDto, TaskQueryDto } from '../../api/task';
import type { SelectOptionDto } from '../../api/common';

const router = useRouter();

const squareTasks = ref<TaskListDto[]>([]);
const skillOptions = ref<SelectOptionDto[]>([]);

const QueryForm = reactive<TaskQueryDto>({
  TaskName: '',
  ProjectName: '',
  Skills: []
});

const handleSearch = async () => {
  try {
    const res = await getTaskSquare(QueryForm);
    squareTasks.value = res as any;
  } catch (error) {
    console.error('获取待分配任务失败', error);
  }
};

const handleApply = async (taskId: number) => {
  try {
    await ElMessageBox.confirm('确定申请该任务吗？', '提示');
    
    await handleApplication(taskId);
    ElMessage.success('任务申请已提交，请在"我的申请"页面查看申请状态');
    
    handleSearch();
  } catch (err) {
    // 异常
  }
};


const goDetail = (taskId: number) => {
  router.push(`/task/detail/${taskId}`);
};


onMounted(async () => {
  const res = await getCategories('tags');
  skillOptions.value = res as any;
  
  handleSearch();
});
</script>

<style scoped>
.el-table {
  --el-table-header-bg-color: #f5f7fa;
}
</style>