<template>
  <div style="width: 95%; margin: 20px auto;">
    <el-card style="margin-top: 20px;" shadow="never">
      <el-table :data="reviewList" border stripe v-loading="loading">
        <el-table-column prop="ReviewId" label="编号" width="60" align="center" />
        
        <el-table-column label="关联任务" width="180" align="center">
          <template #default="scope">
            <el-link type="primary" @click="handleToDetail(scope.row)">
              {{ scope.row.TaskName }}
            </el-link>
          </template>
        </el-table-column>

        <el-table-column prop="Version" label="版本" width="80" align="center">
          <template #default="scope">
            <el-tag effect="plain" size="small">V{{ scope.row.Version }}</el-tag>
          </template>
        </el-table-column>
        
        <el-table-column label="任务提交成果" width="200" align="center">
          <template #default="scope">
            <el-space spacer="|">
              <el-link 
                v-if="scope.row.GitUrl" 
                type="primary" 
                :href="scope.row.GitUrl" 
                target="_blank"
              >Git链接</el-link>
              
              <el-link 
                v-if="scope.row.ArchiveUrl" 
                type="success" 
                @click="handleDownload(scope.row.ArchiveUrl)"
              >程序源文件</el-link>

              <el-link 
                v-if="scope.row.DocUrl" 
                type="warning" 
                @click="handleDownload(scope.row.DocUrl)"
              >技术文档</el-link>
              
              <span v-if="!scope.row.GitUrl && !scope.row.ArchiveUrl" style="color: #999">无</span>
            </el-space>
          </template>
        </el-table-column>

        <el-table-column label="评审状态" width="100" align="center">
          <template #default="scope">
            <el-tag :type="getStatusType(scope.row.Result)">
              {{ getStatusText(scope.row.Result) }}
            </el-tag>
          </template>
        </el-table-column>

        <el-table-column prop="Comment" label="评审意见" width="250" align="center"/>
        <el-table-column prop="PmName" label="评审人" width="100" align="center" />

        <el-table-column prop="ReviewTime" label="评审时间" width="150" align="center">
          <template #default="scope">
            <span>
              {{ scope.row.ReviewTime ? $dayjs(scope.row.ReviewTime).format('YYYY-MM-DD HH:mm') : '--' }}
            </span>
          </template>
        </el-table-column>

        <el-table-column label="操作" width="100" fixed="right" align="center">
          <template #default="scope">
            <el-button 
              v-if="userRole === 2 && scope.row.Result === 1" 
              type="primary" 
              size="small" 
              @click="handleReview(scope.row.ReviewId)"
            >
              评审该任务
            </el-button>
            <span v-else>--</span>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <ApproveDialog ref="approveRef" @refresh="loadReviews" />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { ElMessage } from 'element-plus';
import { useRouter } from 'vue-router';
import { getReviewHistory } from '../../api/review';
import { downloadFile } from '../../api/common';
import { saveAs } from '../../utils/file';
import type { ReviewListDto } from '../../api/review';
import ApproveDialog from '../../components/approve.vue';

const router = useRouter();
const userRole = Number(localStorage.getItem('userRole'));

const loading = ref(false);
const reviewList = ref<ReviewListDto[]>([]);
const approveRef = ref();


const handleToDetail = (rowItem: any) => {
  const targetId = rowItem.TaskId || rowItem.taskId; 

  if (targetId) {
    router.push(`/task/detail/${targetId}`);
  }
  else {
    ElMessage.warning("查询不到该任务");
  }
};

const handleDownload = async (fileUrl: string) => {
  if (!fileUrl) return;
  try {
    const res = await downloadFile(fileUrl);
    const name = fileUrl.split('/').pop() || '提交文件';
    saveAs(res as any, name);
  } catch (error) {
    console.error('任务提交文件下载失败:', error);
  }
};

const loadReviews = async () => {
  loading.value = true;
  try {
    const res = await getReviewHistory();
    reviewList.value = res as any;
  } catch (err) {
    console.error('加载任务评审列表失败', err);
  } finally {
    loading.value = false;
  }
};

const getStatusText = (result: number) => {
  const map: Record<number, string> = { 1: '待评审', 2: '通过', 3: '驳回' };
  return map[result] || '未知';
};

const getStatusType = (result: number) => {
  const map: Record<number, string> = { 1: 'info', 2: 'success', 3: 'danger' };
  return map[result] || '';
};

const handleReview = (id: number) => {
  approveRef.value.open(id, 'review');
};

onMounted(() => {
  loadReviews();
});
</script>