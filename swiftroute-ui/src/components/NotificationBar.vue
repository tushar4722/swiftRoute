<script setup>
import { computed } from 'vue'
import { useSystemStore } from '@/stores/systemStore'

const store = useSystemStore()
const notifications = computed(() => store.notifications)
</script>

<template>
  <div class="notification-container">
    <TransitionGroup name="slide">
      <div v-for="notif in notifications" :key="notif.id" class="notification-toast glass-panel">
        <div class="notif-icon">🔔</div>
        <div class="notif-content">
          <strong>System Alert</strong>
          <p>{{ notif.text }}</p>
        </div>
        <button class="close-btn" @click="store.removeNotification(notif.id)">✕</button>
      </div>
    </TransitionGroup>
  </div>
</template>

<style scoped>
.notification-container {
  position: fixed;
  top: 80px;
  right: 20px;
  display: flex;
  flex-direction: column;
  gap: 10px;
  z-index: 9999;
}

.notification-toast {
  display: flex;
  align-items: center;
  gap: 15px;
  padding: 1rem;
  width: 350px;
  background: rgba(26, 31, 38, 0.9) !important;
  border-left: 4px solid var(--danger-color) !important;
}

.notif-icon {
  font-size: 1.5rem;
}

.notif-content p {
  color: var(--text-secondary);
  font-size: 0.9rem;
  margin-top: 2px;
}

.close-btn {
  margin-left: auto;
  background: none;
  border: none;
  color: var(--text-secondary);
  cursor: pointer;
  font-size: 1.2rem;
}

.close-btn:hover {
  color: #fff;
}

.slide-enter-active,
.slide-leave-active {
  transition: all 0.4s ease;
}
.slide-enter-from {
  opacity: 0;
  transform: translateX(50px);
}
.slide-leave-to {
  opacity: 0;
  transform: translateX(50px);
}
</style>
