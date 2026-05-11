<template>
  <el-dialog
    v-model="visible"
    title="调整任务预估工时"
    width="480px"
    destroy-on-close
  >
    <el-form 
      ref="formRef"
      :model="form" 
      :rules="rules" 
      label-width="100px" 
      label-position="top"
      style="padding: 0 20px"
    >
      <el-row :gutter="20" style="margin-bottom: 20px; background: #f5f7fa; padding: 15px; border-radius: 4px;">
        <el-col :span="11" style="text-align: center;">
          <div style="font-size: 12px; color: #909399;">当前工时</div>
          <div style="font-size: 18px; font-weight: bold;">{{ oldHours }} h</div>
        </el-col>
        <el-col :span="2" style="display: flex; align-items: center; justify-content: center; color: #dcdfe6;">
          <el-icon><Right /></el-icon>
        </el-col>
        <el-col :span="11" style="text-align: center;">
          <div style="font-size: 12px; color: #909399;">调整后</div>
          <div style="font-size: 18px; font-weight: bold; color: #409eff;">{{ form.NewEstimatedHours }} h</div>
        </el-col>
      </el-row>

      <el-form-item label="新的预估工时" prop="NewEstimatedHours">
        <el-input-number 
          v-model="form.NewEstimatedHours" 
          :min="1" 
          :step="1" 
          controls-position="right"
          style="width: 100%"
        />
      </el-form-item>

      <el-form-item label="调整原因" prop="ChangeReason">
        <el-input
          v-model="form.ChangeReason"
          type="textarea"
          :rows="4"
          placeholder="请输入调整原因"
        />
      </el-form-item>
    </el-form>

    <template #footer>
      <span class="dialog-footer">
        <el-button @click="visible = false">取消</el-button>
        <el-button type="primary" :loading="loading" @click="submit">确认提交</el-button>
      </span>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { ElMessage } from 'element-plus'
import { Right } from '@element-plus/icons-vue'
import { updateTaskHours } from '@/api/task'

const visible = ref(false)
const loading = ref(false)
const formRef = ref()
const taskId = ref<number | null>(null)
const oldHours = ref(0)

// 表单数据
const form = reactive({
  NewEstimatedHours: 0,
  ChangeReason: ''
})

// 校验规则
const rules = {
  NewEstimatedHours: [{ required: true, message: '请输入更新后的预估工时', trigger: 'blur' }],
  ChangeReason: [
    { required: true, message: '请填写原因', trigger: 'blur' }
  ]
}

// 表单打开时填入的基础数据
const open = (id: number, currentHours: number) => {
  taskId.value = id
  oldHours.value = currentHours
  form.NewEstimatedHours = currentHours
  form.ChangeReason = ''
  visible.value = true
}

// 提交
const submit = async () => {
  if (!formRef.value) return
  
  await formRef.value.validate(async (valid: boolean) => {
    if (!valid) return
    
    try {
      loading.value = true
      await updateTaskHours(taskId.value!, {
        NewEstimatedHours: form.NewEstimatedHours,
        ChangeReason: form.ChangeReason
      })
      ElMessage.success('任务预估工时修改成功')
      visible.value = false
      emit('success') 
    } catch (error) {
      console.error('更新失败:', error)
    } finally {
      loading.value = false
    }
  })
}

const emit = defineEmits(['success'])

// 公开方法至父组件
defineExpose({ open })
</script>