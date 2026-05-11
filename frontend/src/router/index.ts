import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import Layout from '@/layout/index.vue'

const routes: Array<RouteRecordRaw> = [
  // 登录
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/views/login/Login.vue')
  },
  // 注册
  {
    path: '/register',
    name: 'Register',
    component: () => import('@/views/login/Register.vue') 
  },

  // 核心业务，全部嵌在Layout下
  {
    path: '/',
    component: Layout,
    redirect: () => {
      const token = localStorage.getItem('token');
      const userRole = localStorage.getItem('userRole'); 
      if (!token) return '/login';

      if (userRole === '1' || userRole === '2') {
        return '/project/index?type=pending'; 
      }
      else if(userRole === '3'){
        return '/task/index'; 
      }
      return '/project/index'; 
    },

    children: [
      // 个人中心
      {
        path: 'profile/index',
        name: 'Profile',
        component: () => import('@/views/profile/index.vue'),
        meta: { title: '个人中心' } 
      },
      {
        path: 'profile/update',
        name: 'ProfileUpdate',
        component: () => import('@/views/profile/update.vue'),
        meta: { title: '个人资料修改' } 
      },
      // 项目管理
      {
        path: 'project/index',
        name: 'ProjectList',
        component: () => import('@/views/project/index.vue'),
        meta: { title: '项目列表' } 
      },
      {
        path: 'project/detail/:id',
        name: 'ProjectDetail',
        component: () => import('@/views/project/detail.vue'),
        meta: { title: '项目详情' } 
      },
      {
        path: 'project/edit/:id?',
        name: 'ProjectEdit',
        component: () => import('@/views/project/edit.vue'),
        meta: { title: '添加/修改项目' } 
      },
      // 任务管理
      {
        path: 'task/index',
        name: 'TaskList',
        component: () => import('@/views/task/index.vue'),
        meta: { title: '任务列表' } 
      },
      {
        path: 'task/detail/:id',
        name: 'TaskDetail',
        component: () => import('@/views/task/detail.vue'),
        meta: { title: '任务详情' } 
      },
      {
        path: 'task/application',
        name: 'TaskApplication',
        component: () => import('@/views/task/application.vue'),
        meta: { title: '任务申请列表' } 
      },
      {
        path: 'task/submit/:id',
        name: 'TaskSubmit',
        component: () => import('@/views/task/submit.vue'),
        meta: { title: '任务成果提交' } 
      },
      {
        path: 'task/dev-apply',
        name: 'TaskDevApply',
        component: () => import('@/views/task/dev-apply.vue'),
        meta: { title: '任务广场' } 
      },
      {
        path: 'task/review',
        name: 'TaskReview',
        component: () => import('@/views/task/review.vue'),
        meta: { title: '任务评审' } 
      },
      // 用户管理
      {
        path: 'user/index',
        name: 'UserList',
        component: () => import('@/views/user/index.vue'),
        meta: { title: '用户列表' } 
      },
      {
        path: 'user/detail/:id',
        name: 'UserDetail',
        component: () => import('@/views/user/detail.vue'),
        meta: { title: '用户详情' } 
      },
      // 工时日志
      {
        path: 'log/work-log',
        name: 'WorkLog',
        component: () => import('@/views/log/work-log.vue'),
        meta: { title: '开发人员工时日志' } 
      },
      {
        path: 'log/task-change-log',
        name: 'TaskChangeLog',
        component: () => import('@/views/log/task-change-log.vue'),
        meta: { title: 'PM工时日志修改审计' } 
      },
      // 绩效管理
      {
        path: 'performance/pending',
        name: 'PerformancePending',
        component: () => import('@/views/performance/pending.vue'),
        meta: { title: '待评分绩效' } 
      },
      {
        path: 'performance/index',
        name: 'PerformanceIndex',
        component: () => import('@/views/performance/index.vue'),
        meta: { title: '已发布绩效' } 
      },
      // 数据统计
      {
        path: 'stats/project-progress',
        name: 'StatProjectProgress',
        component: () => import('@/views/stats/project-progress.vue'),
        meta: { title: '项目大盘' } 
      },
      {
        path: 'stats/work-hours-audit',
        name: 'StatWorkHoursAudit',
        component: () => import('@/views/stats/work-hours-audit.vue'),
        meta: { title: '工时成本偏差' } 
      },
      {
        path: 'stats/user-capability',
        name: 'StatUserCapability',
        component: () => import('@/views/stats/user-capability.vue'),
        meta: { title: '个体能力画像' } 
      },
      {
        path: 'stats/dev-efficiency',
        name: 'StatDevEfficiency',
        component: () => import('@/views/stats/dev-efficiency.vue'),
        meta: { title: '开发人员效能' } 
      },
      // 消息通知
      {
        path: 'notice/index',
        name: 'NoticeIndex',
        component: () => import('@/views/notice/index.vue'),
        meta: { title: '收件箱' } 
      }
    ]
  },

  {
    path: '/:pathMatch(.*)*',
    redirect: '/'
  }
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
})

router.beforeEach((to, _from, next) => {
  const token = localStorage.getItem('token')
  const whiteList = ['/login', '/register']

  // 登录、注册页，直接放行
  if (whiteList.includes(to.path)) {
    if (token && to.path === '/login') {
      next('/project/index')
    } else {
      next()
    }
  } 
  // 其他页面，若无有效token，则拦截
  else if (!token) {
    next('/login')
  } 
  // 根路径的重定向
  else if (to.path === '/') {
    next('/project/index')
  }
  else {
    next()
  }
})

router.afterEach((to) => {
  const title = to.meta.title as string;
  document.title = title ? `${title} - 软件外包管理系统` : '软件外包管理系统';
});

export default router;