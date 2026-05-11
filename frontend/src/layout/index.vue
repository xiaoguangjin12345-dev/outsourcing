<template>
  <el-container class="app-wrapper">
    <el-aside width="220px" class="sidebar-container">
      <div class="logo-box">
        <span class="logo-text">软件外包项目管理系统</span>
      </div>
      
      <el-menu
        :default-active="activeMenu"
        router
        background-color="#304156"
        text-color="#fff"
        active-text-color="#409EFF"
        :unique-opened="true"
      >
        <el-sub-menu v-if = "userRole !== '3'" index="project">
          <template #title>
            <span>项目管理</span>
          </template>

          <el-menu-item v-if="userRole === '4'" index="/project/index">
            项目列表
          </el-menu-item>

          <el-menu-item v-if="userRole === '1'" index="/project/index?type=pending">
            项目立项审核
          </el-menu-item>
          <el-menu-item v-if="userRole === '2'" index="/project/index?type=pending">
            项目立项申请
          </el-menu-item>
          <el-menu-item v-if="['1', '2'].includes(userRole)" index="/project/index?type=pass">
            已审批项目
          </el-menu-item>

        </el-sub-menu>


        <el-sub-menu index="task">
          <template #title>
            <span>任务管理</span>
          </template>

          <el-menu-item index="/task/index">
            我的任务列表
          </el-menu-item>

          <el-menu-item v-if="['1', '4'].includes(userRole)" index="/task/application">
            任务申请列表
          </el-menu-item>

          <el-menu-item v-if="userRole === '2'" index="/task/application?direction=my">
            我的任务邀请
          </el-menu-item>
          <el-menu-item v-if="userRole === '2'" index="/task/application?direction=you">
            开发人员申请
          </el-menu-item>

          <el-menu-item v-if="userRole === '3'" index="/task/application?direction=my">
            我的任务申请
          </el-menu-item>
          <el-menu-item v-if="userRole === '3'" index="/task/application?direction=you">
            项目经理邀请
          </el-menu-item>
          <el-menu-item v-if="userRole === '3'" index="/task/dev-apply">
            任务广场
          </el-menu-item>

          <el-menu-item index="/task/review">
            任务评审
          </el-menu-item>
        </el-sub-menu>



        <el-sub-menu v-if = "['1', '4'].includes(userRole)" index="user">
          <template #title>
            <span>用户管理</span>
          </template>

          <el-menu-item v-if="userRole === '1'" index="/user/index">
            用户列表
          </el-menu-item>
          <el-menu-item v-if="userRole === '4'" index="/user/index?type=pending">
            待审核用户
          </el-menu-item>
          <el-menu-item v-if="userRole === '4'" index="/user/index?type=pass">
            已审核用户
          </el-menu-item>

        </el-sub-menu>



        <el-sub-menu index="log">
          <template #title>
            <span>工时日志</span>
          </template>

          <el-menu-item index="/log/work-log">
            开发人员工时日志
          </el-menu-item>
          <el-menu-item v-if="['1', '4'].includes(userRole)" index="/log/task-change-log">
            PM任务预估审计
          </el-menu-item>

        </el-sub-menu>



        <el-sub-menu index="performance">
          <template #title>
            <span>绩效管理</span>
          </template>

          <el-menu-item v-if="userRole !== '3'" index="/performance/pending">
            待评分绩效
          </el-menu-item>
          <el-menu-item index="/performance/index">
            已发布绩效
          </el-menu-item>

        </el-sub-menu>


        <el-sub-menu index="notice">
          <template #title>
            <span>
              消息通知
              <el-badge 
                v-if="unreadCount > 0" 
                :value="unreadCount" 
                :max="99" 
                style="margin-left: 18px; position: relative; top: -18px;"
              />
            </span>
          </template>

          <el-menu-item index="/notice/index">
            收件箱
          </el-menu-item>
        </el-sub-menu>

        <el-sub-menu index="stats">
          <template #title>
            <span>数据统计</span>
          </template>

          <el-menu-item index="/stats/project-progress">
            项目大盘
          </el-menu-item>
          <el-menu-item v-if="userRole !== '3'" index="/stats/work-hours-audit">
            工时偏差
          </el-menu-item>
          <el-menu-item index="/stats/user-capability">
            个体能力画像
          </el-menu-item>
          <el-menu-item v-if="userRole !== '3'" index="/stats/dev-efficiency">
            开发人员效能
          </el-menu-item>

        </el-sub-menu>

      </el-menu>


    </el-aside>

    <el-container>
      <el-header class="navbar">
        <div class="left-menu">
          <span>当前位置：{{ route.meta.title || currentRouteName }}</span>
        </div>
        
        <div class="right-menu">

          <el-dropdown>
            <span class="user-name-link">
              {{ userName }} 【{{ roleName }}】
            </span>

            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item @click="router.push('/profile/index')">
                  个人中心
                </el-dropdown-item>
      
                <el-dropdown-item divided @click="handleLogout">
                  退出登录
                </el-dropdown-item>

              </el-dropdown-menu>
            </template>
          </el-dropdown>

        </div>
      </el-header>

      <el-main class="app-main">
        <router-view :key="route.fullPath" />
      </el-main>

    </el-container>
  </el-container>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { ElMessageBox } from 'element-plus';
import { getUnreadCount } from '@/api/notice';
import request from '../utils/request';

const route = useRoute();
const router = useRouter();

const userRole = localStorage.getItem('userRole') || '';
const userName = localStorage.getItem('userName') || '未登录';

const activeMenu = computed(() => route.fullPath); 
const currentRouteName = computed(() => route.meta.title || '详情');
const roleName = computed(() => {
  const roles: Record<string, string> = { '1': 'PMO', '2': '项目经理', '3': '开发人员', '4': '管理员' };
  return roles[userRole] || '访客';
});

const handleLogout = () => {
  ElMessageBox.confirm('确定要退出系统吗？', '提示').then(() => {
    localStorage.clear();
    router.replace('/login');
  });
};

// 未读消息总数轮询查询
const unreadCount = ref(0);
let timer: number | undefined;

const fetchCount = async () => {
  try {
    const res = await getUnreadCount();
    unreadCount.value = res as any; 
  } catch (err) {
    console.error("轮询消息失败", err);
  }
};

onMounted(() => {
  fetchCount();
  timer = window.setInterval(fetchCount, 10000); // 10秒
});

onUnmounted(() => {
  if (timer) clearInterval(timer);
});

</script>

<style scoped>
.app-wrapper { height: 100vh; width: 100%; display: flex; }
.sidebar-container { background-color: #304156; color: #fff; }
.logo-box { height: 50px; line-height: 50px; text-align: center; background: #2b2f3a; }
.navbar { 
  height: 50px; 
  display: flex; 
  align-items: center; 
  justify-content: space-between; 
  padding: 0 20px; 
  border-bottom: 1px solid #ddd;
}
.right-menu { display: flex; align-items: center; gap: 20px; }
.user-name-link { cursor: pointer; display: flex; align-items: center; gap: 5px; }
.app-main { background-color: #f5f7f9; padding: 20px; }
</style>