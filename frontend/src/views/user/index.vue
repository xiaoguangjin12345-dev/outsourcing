<template>
  <div class="page-container" style="width: 95%; margin: 20px auto;">
    <el-card shadow="never">
      <el-form :inline="true" :model="queryForm">
        <el-form-item label="真实姓名">
          <el-input 
            v-model="queryForm.RealName" 
            placeholder="搜索" 
            clearable 
            @keyup.enter="fetchData" 
          />
        </el-form-item>
        
        <el-form-item v-if="!isInviteMode" label="角色">
          <el-select v-model="queryForm.Roles" multiple collapse-tags placeholder="全部" style="width: 200px">
            <el-option label="PMO" :value="1" />
            <el-option label="项目经理" :value="2" />
            <el-option label="开发人员" :value="3" />
          </el-select>
        </el-form-item>

        <el-form-item label="技能标签">
          <el-select
            v-model="queryForm.Tags"
            multiple
            collapse-tags
            collapse-tags-tooltip
            :max-collapse-tags="5" 
            placeholder="全部"
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
          <el-button type="primary" @click="fetchData">查询</el-button>
          <el-button @click="resetQuery">重置查询条件</el-button>
        </el-form-item>
      </el-form>

      <el-table v-loading="loading" :data="userList" border stripe>
        <el-table-column prop="UserId" label="编号" width="80" align="center" />
        <el-table-column prop="RealName" label="姓名" width="120" />
        
        <el-table-column label="技能标签" show-overflow-tooltip>
          <template #default="{ row }">
            <el-tag 
              v-for="s in (row.Skills?.split(',') || [])" 
              :key="s" 
              size="small" 
              style="margin-right: 5px"
            >
              {{ s }}
            </el-tag>
          </template>
        </el-table-column>
        
        <el-table-column v-if = "[1, 4].includes(userRole)" label="角色" width="120">
          <template #default="{ row }">
            <el-tag effect="plain" type="info">{{ getRoleName(row.Role) }}</el-tag>
          </template>
        </el-table-column>

        <el-table-column label="操作" :width="isInviteMode ? 150 : 120" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" @click="goDetail(row.UserId)">详情</el-button>

            <el-button 
              v-if="isInviteMode" 
              type="success" 
              size="small" 
              @click="onInvite(row)"
            >
              邀请
            </el-button>

            <el-button 
              v-if="userRole === 4 && route.query.type === 'pending'" 
              link type="primary" 
              @click="approveRef.open(row.UserId, 'user-audit')"
            >
              审核用户
            </el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <ApproveDialog ref = "approveRef" @refresh="fetchData" />
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ElMessage, ElMessageBox } from 'element-plus';
import { getUsers } from '@/api/user';
import { handleApplication } from '@/api/task';
import type { SelectOptionDto } from '@/api/common';
import { getCategories } from '@/api/common';
import ApproveDialog from '@/components/approve.vue';

const route = useRoute();
const router = useRouter();

const userRole = Number(localStorage.getItem('userRole'));
const loading = ref(false);
const userList = ref([]);
const approveRef = ref();

const tagOptions = ref<SelectOptionDto[]>([]); 

const taskId = computed(() => Number(route.query.taskId));
const isInviteMode = computed(() => !!taskId.value);

const queryForm = reactive({
  RealName: '',
  Roles: [] as number[],
  Status: [] as number[],
  Tags: [] as number[]
});

const fetchData = async () => {
  loading.value = true;
  try {
    const params: any = {
      RealName: queryForm.RealName,
      Roles: queryForm.Roles,
      Statuses: queryForm.Status,
      Skills: queryForm.Tags
    };

    if (isInviteMode.value) {
      params.Roles = [3];
    }

    const res = await getUsers(params);
    userList.value = (res as any).data || res;
  } catch (error) {
    console.error('Fetch users error:', error);
  } finally {
    loading.value = false;
  }
};

const resetQuery = () => {
  queryForm.RealName = '';
  queryForm.Roles = [];
  queryForm.Tags = [];
  fetchData();
};

const goDetail = (id: number) => {
  router.push(`/user/detail/${id}`);
};

const onInvite = async (user: any) => {
  try {
    await ElMessageBox.confirm(`确认邀请 ${user.RealName} 吗？`, '邀请');
    await handleApplication(taskId.value, { DevID: user.UserId });
    ElMessage.success('任务邀请提交成功');
    router.push('/task/index'); 
  } catch (e) {}
};

const getRoleName = (id: number) => {
  const map: Record<number, string> = { 1: 'PMO', 2: '项目经理', 3: '开发人员', 4: '系统管理员' };
  return map[id] || '未知';
};

onMounted(async () => {
  try {
    const tag = await getCategories('tags');
    tagOptions.value = tag as unknown as SelectOptionDto[];
  } catch (error) {
    console.error('加载下拉选项框失败', error);
  }

  if (userRole === 4){
    if(route.query.type === 'pending'){
      queryForm.Status = [1];
    }else{
      queryForm.Status = [2];
    }
  }
  else if(userRole === 1){
    queryForm.Status = [1, 2];
  }
  else if(userRole === 2){
    if (route.query.type === 'talent-pool'){
      queryForm.Status = [2];
    }
  }
  
  fetchData();
});
</script>