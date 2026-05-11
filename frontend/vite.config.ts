import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [
    vue(),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    },
  },
  server: {
    port: 5174,    
    strictPort: true, // 强制使用该端口
    proxy: {
      '/api': {
        target: 'http://localhost:5013',   // C#后端地址
        changeOrigin: true,               // 允许跨域
      }
    }
  }
})
