<template>
  <div style="width: 95%; margin: 20px auto;">
    <el-card style="margin-top: 20px;" shadow="never">
      <el-form :inline="true" :model="queryForm">
        <el-form-item label="发送人">
          <el-input v-model="queryForm.SenderName" placeholder="搜索姓名" clearable style="width: 150px" />
        </el-form-item>
        
        <el-form-item label="状态">
          <el-select v-model="queryForm.Statuses" multiple collapse-tags placeholder="全部" style="width: 180px">
            <el-option 
              v-for="opt in statusOptions" 
              :key="opt.Value" 
              :label="opt.Label" 
              :value="Number(opt.Value)" 
            />
          </el-select>
        </el-form-item>

        <el-form-item label="类型">
          <el-select v-model="queryForm.NoticeTypes" multiple collapse-tags placeholder="全部" style="width: 180px">
            <el-option 
              v-for="opt in typeOptions" 
              :key="opt.Value" 
              :label="opt.Label" 
              :value="Number(opt.Value)" 
            />
          </el-select>
        </el-form-item>

        <el-button type="primary" @click="fetchData">查询</el-button>
        <el-button @click="resetQuery">重置</el-button>
      </el-form>
    </el-card>

    <el-table :data="noticeList" border stripe style="margin-top: 20px" v-loading="loading">
      <el-table-column label="状态" width="100" align="center">
        <template #default="scope">
          <el-badge is-dot :hidden="scope.row.Status !== 1">
            <el-tag :type="scope.row.Status === 1 ? 'danger' : 'info'" size="small">
              {{ translate(statusOptions, scope.row.Status) }}
            </el-tag>
          </el-badge>
        </template>
      </el-table-column>
      
      <el-table-column label="消息类型" width="120" align="center">
        <template #default="{ row }">
          <el-tag :color="getTypeColor(row.NoticeType)" effect="dark" border="false" size="small" style="color: white; border: none">
            {{ translate(typeOptions, row.NoticeType) }}
          </el-tag>
        </template>
      </el-table-column>

      <el-table-column prop="SenderName" label="来自" width="120" align="center"/>
      
      <el-table-column prop="Content" label="内容摘要" width="500" align="center">
        <template #default="{ row }">
          <span :class="{ 'unread-text': row.Status === 1 }">{{ row.Content }}</span>
        </template>
      </el-table-column>
      
      <el-table-column prop="CreateTime" label="发送时间" min-width="80" align="center">
        <template #default="scope">
          <span>
            {{ scope.row.CreateTime ? $dayjs(scope.row.CreateTime).format('YYYY-MM-DD HH:mm') : '--' }}
          </span>
        </template>
      </el-table-column>

      <el-table-column label="操作" width="160" fixed="right" align="center">
        <template #default="scope">
          <el-button size="small" link type="primary" @click="handleOpenDetail(scope.row)">详情</el-button>
          <el-button 
            v-if="userRole !== 4" 
            size="small" 
            link
            type="danger" 
            @click="handleDelete(scope.row.NoticeId)"
          >
            删除
          </el-button>
        </template>
      </el-table-column>
    </el-table>

    <NoticeDetail ref="detailRef" @refresh="fetchData" />
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue';
import { ElMessage, ElMessageBox } from 'element-plus';
import { getInbox, deleteNotice } from '../../api/notice';
import { getCategories } from '../../api/common';
import type { SelectOptionDto } from '../../api/common';
import NoticeDetail from '../../components/notice-detail.vue';

const userRole = Number(localStorage.getItem('userRole'));
const noticeList = ref([]);
const statusOptions = ref<SelectOptionDto[]>([]);
const typeOptions = ref<SelectOptionDto[]>([]);
const detailRef = ref();
const loading = ref(false);

const queryForm = reactive({
  SenderName: '',
  Statuses: [] as number[],
  NoticeTypes: [] as number[]
});


const translate = (options: any[], val: any) => {
  if (!options || options.length === 0) return val;
  const target = options.find(o => String(o.Value) === String(val));
  return target ? target.Label : val;
};


const getTypeColor = (type: number) => {
  const map: Record<number, string> = {
    1: '#409EFF', // 系统通知 - 蓝
    2: '#67C23A', // 审核通知 - 绿
    3: '#E6A23C', // 申请通知 - 黄
    4: '#F56C6C', // 工时预警 - 红
    5: '#909399', // 验收通知 - 灰
  };
  return map[type] || '#909399';
};

// 初始化
const initDicts = async () => {
  try {
    const [sRes, tRes] = await Promise.all([
      getCategories('notice-status'),
      getCategories('notice-type')
    ]);
    statusOptions.value = sRes as any;
    typeOptions.value = tRes as any;
  } catch (err) {
    console.error("字典加载失败", err);
  }
};

// 数据获取
const fetchData = async () => {
  loading.value = true;
  try {
    const res = await getInbox(queryForm);
    noticeList.value = res as any;
  } finally {
    loading.value = false;
  }
};

const resetQuery = () => {
  queryForm.SenderName = '';
  queryForm.Statuses = [];
  queryForm.NoticeTypes = [];
  fetchData();
};

const handleOpenDetail = (row: any) => {
  detailRef.value.open(row.NoticeId);
};

const handleDelete = async (id: number) => {
  try {
    await ElMessageBox.confirm('确定要删除此消息吗？', '提示', { type: 'warning' });
    await deleteNotice(id); 
    ElMessage.success('已删除该消息');
    fetchData();
  } catch (err) {}
};

onMounted(async () => {
  await initDicts();
  fetchData();
});
</script>

<style scoped>
.unread-text {
  font-weight: bold;
  color: #303133;
}
:deep(.el-badge__content.is-fixed.is-dot) {
  right: 5px;
  top: 2px;
}
</style>