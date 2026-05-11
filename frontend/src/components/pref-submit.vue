<template>
  <el-dialog v-model="visible" :title="dialogTitle" width="450px" destroy-on-close>
    <el-form :model="form" label-width="100px" style="padding: 10px">
      
      <el-form-item label="主观评分" required>
        <el-input-number 
          v-model="form.SubjectiveScore" 
          :min="0" 
          :max="100" 
          controls-position="right"
        />
        <div class="form-tip">范围: 0 - 100 分</div>
      </el-form-item>

      <el-form-item label="评价意见">
        <el-input 
          v-model="form.Comment" 
          type="textarea" 
          :rows="4" 
          placeholder="请填写绩效评价意见" 
        />
      </el-form-item>
    </el-form>
    
    <template #footer>
      <el-button @click="visible = false">取消</el-button>
      <el-button type="primary" :loading="submitting" @click="handleSubmit">
        提交绩效主观评定部分
      </el-button>
    </template>

    
  </el-dialog>
</template>

<script setup lang="ts">
import { ref, reactive, computed } from 'vue';
import { ElMessage } from 'element-plus';
import { submitPerformanceScore } from '@/api/performance';
import type { PerformanceScoreDto } from '@/api/performance';

const emit = defineEmits(['refresh']);
const visible = ref(false);
const submitting = ref(false);
const currentId = ref(0);
const perfType = ref(1); // 1-项目, 2-任务


const form = reactive<PerformanceScoreDto>({
  SubjectiveScore: 80,
  Comment: ''
});

const dialogTitle = computed(() => `${perfType.value === 1 ? '项目级' : '任务级'}绩效评价`);

const open = (row: any) => {
  currentId.value = row.PerformanceId;
  perfType.value = row.PerformanceType;
  form.SubjectiveScore = 80;
  form.Comment = '';
  visible.value = true;
};

const handleSubmit = async () => {
  if (form.SubjectiveScore === null) return ElMessage.warning('请输入主观评价部分得分');
  
  submitting.value = true;
  try {
    await submitPerformanceScore(currentId.value, form);
    
    ElMessage.success('评分提交成功，绩效已自动结算并归档');
    visible.value = false;
    emit('refresh');
  } catch (error) {
    // 错误由request拦截器统一处理
  } finally {
    submitting.value = false;
  }
};

defineExpose({ open });
</script>

<style scoped>
.form-tip {
  font-size: 12px;
  color: #909399;
  margin-top: 4px;
}
</style>