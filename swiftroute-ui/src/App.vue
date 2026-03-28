<script setup>
import { RouterView, useRoute, useRouter } from 'vue-router'
import NotificationBar from '@/components/NotificationBar.vue'

const route = useRoute()
const router = useRouter()
</script>

<template>
  <div class="sidebar">
    <div class="logo-area">
      <div class="logo-title">Logistics Hub</div>
      <div class="logo-sub">Precision Architect</div>
    </div>
    
    <div class="sidebar-nav">
      <div class="nav-item" :class="{ active: route.path === '/' }" @click="router.push('/')">
        <span class="nav-icon">⊞</span> Dashboard
      </div>
      <div class="nav-item" :class="{ active: route.path === '/fleet' }" @click="router.push('/fleet')">
        <span class="nav-icon">🚛</span> Fleet
      </div>
      <div class="nav-item" :class="{ active: route.path === '/drivers' }" @click="router.push('/drivers')">
        <span class="nav-icon">👤</span> Drivers
      </div>
      <div class="nav-item">
        <span class="nav-icon">🗺️</span> Routes
      </div>
      <div class="nav-item" :class="{ active: route.path === '/maintenance' }" @click="router.push('/maintenance')">
        <span class="nav-icon">🔧</span> Maintenance
      </div>
      <div class="nav-item">
        <span class="nav-icon">📊</span> Reports
      </div>
    </div>

    <div class="dispatch-btn-container">
      <button class="btn-new-dispatch">↻ New Dispatch</button>
    </div>

    <div class="sidebar-nav mt-auto">
      <div class="nav-item">
        <span class="nav-icon">❓</span> Support
      </div>
      <div class="nav-item">
        <span class="nav-icon">🚪</span> Logout
      </div>
    </div>
  </div>

  <main class="main-wrapper animate-fade-in">
    <NotificationBar />

    <header class="topbar">
      <div style="display: flex; align-items: center; width: 100%;">
        <div class="brand-text" v-if="route.path === '/'">SwiftRoute</div>
        <input type="text" class="search-box" :placeholder="route.path === '/maintenance' ? 'Search fleet or logs...' : 'Search manifest...'" />
        
        <div class="topbar-icons" style="margin-left: auto;">
          <span style="font-size: 1.25rem;">🔔</span>
          <span style="font-size: 1.25rem;">⚙️</span>
          <span style="font-size: 1.25rem;">❓</span>
          <div style="display: flex; align-items: center; gap: 0.5rem; border-left: 1px solid var(--border-color); padding-left: 1rem; margin-left: 0.5rem;">
            <div style="text-align: right; line-height: 1.2;">
              <div style="font-size: 0.8rem; font-weight: 700;">Alex Rivera</div>
              <div style="font-size: 0.7rem; color: var(--text-muted);">Fleet Manager</div>
            </div>
            <div style="width: 32px; height: 32px; background: #e9ecef; border-radius: 50%; overflow: hidden; display: flex; align-items: center; justify-content: center;">👤</div>
          </div>
        </div>
      </div>
    </header>

    <div class="content-wrapper">
      <RouterView />
    </div>
  </main>
</template>

<style scoped>
.mt-auto { margin-top: auto; }
.content-wrapper { padding: 2rem; max-width: 1200px; margin: 0 auto; width: 100%; }
</style>