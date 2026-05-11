/// <reference types="vite/client" />
import dayjs from 'dayjs'

declare module '@vue/runtime-core' {
  interface ComponentCustomProperties {
    $dayjs: typeof dayjs
  }
}