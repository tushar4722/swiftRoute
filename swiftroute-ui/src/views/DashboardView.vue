<script setup>
import { computed, onMounted } from 'vue'
import { useSystemStore } from '@/stores/systemStore'

const store = useSystemStore()

onMounted(() => {
  store.fetchAll()
  store.initSignalR()
})

const parseStatusClass = (status) => {
  if (!status) return ''
  const s = status.toUpperCase()
  if (s.includes('PROGRESS')) return 'progress'
  if (s.includes('SCHEDULED') || s.includes('PENDING')) return 'scheduled'
  return ''
}
</script>

<template>
  <div class="page-header">
    <div>
      <h1 class="page-title">Fleet Overview</h1>
      <p class="page-subtitle">Operational status for Monday, October 23rd</p>
    </div>
    <button class="btn-outline">📅 Past 24 Hours</button>
  </div>

  <div class="metrics-grid">
    <div class="metric-card animate-slide-up" style="animation-delay: 0.1s">
      <div class="metric-header">
        <div class="icon-box icon-blue">👥</div>
        <span class="pill pill-info">+12%</span>
      </div>
      <div class="metric-title">Total Drivers</div>
      <div class="metric-value">{{ store.stats.totalDrivers?.toLocaleString() || '0' }}</div>
    </div>

    <div class="metric-card animate-slide-up" style="animation-delay: 0.2s">
      <div class="metric-header">
        <div class="icon-box icon-blue">🚛</div>
        <span class="pill pill-info">Optimal</span>
      </div>
      <div class="metric-title">Total Vehicles</div>
      <div class="metric-value">{{ store.stats.totalVehicles?.toLocaleString() || '0' }}</div>
    </div>

    <div class="metric-card animate-slide-up" style="animation-delay: 0.3s">
      <div class="metric-header">
        <div class="icon-box icon-orange">🛣️</div>
        <span class="pill pill-warning">Live</span>
      </div>
      <div class="metric-title">Active Assignments</div>
      <div class="metric-value">{{ store.stats.activeAssignments?.toLocaleString() || '0' }}</div>
    </div>

    <div class="metric-card animate-slide-up" style="animation-delay: 0.4s">
      <div class="metric-header">
        <div class="icon-box icon-red">⚠️</div>
        <span class="pill pill-warning" style="color:#d08700">Urgent</span>
      </div>
      <div class="metric-title">Maintenance Required</div>
      <div class="metric-value" style="color: #dc3545;">{{ store.stats.maintenanceRequired?.toLocaleString() || '0' }}</div>
    </div>
  </div>

  <div class="layout-grid">
    <div class="alerts-col">
      <h3 style="font-size: 1.1rem; margin-bottom: 0.5rem; display: flex; align-items: center; gap: 0.5rem;">
        <span style="color: var(--danger)">((•))</span> Real-time Alerts
      </h3>
      
      <div class="alert-card animate-slide-in">
        <span class="alert-type">Engine Warning</span>
        <div class="alert-title">Vehicle VH-9284 (Peterbilt)</div>
        <div class="alert-desc">Low oil pressure detected near Chicago Hub. Immediate inspection required.</div>
        <div class="alert-time">2 MINUTES AGO</div>
      </div>

      <div class="alert-card warning animate-slide-in" style="animation-delay: 0.1s">
        <span class="alert-type">Maintenance Scheduled</span>
        <div class="alert-title">Vehicle VH-1102 (Freightliner)</div>
        <div class="alert-desc">Tire rotation due in 450 miles. Dispatch system notifying carrier.</div>
        <div class="alert-time">1 HOUR AGO</div>
      </div>
    </div>

    <div class="map-panel">
      <div class="map-overlay">
        <h4 style="font-size: 0.9rem; color: #1c3664;">Regional Coverage</h4>
        <div class="map-grid">
          <div>
            <div class="map-stat-label">Midwest</div>
            <div class="map-stat-val">312</div>
          </div>
          <div>
            <div class="map-stat-label">East Coast</div>
            <div class="map-stat-val">241</div>
          </div>
        </div>
      </div>
      <!-- Dots simulation -->
      <div style="position: absolute; top:40%; left: 60%; width: 12px; height: 12px; background: #0dcaf0; border-radius: 50%; box-shadow: 0 0 10px #0dcaf0;"></div>
      <div style="position: absolute; top:50%; left: 30%; width: 10px; height: 10px; background: #0d6efd; border-radius: 50%; box-shadow: 0 0 10px #0d6efd;"></div>
      <div style="position: absolute; top:70%; left: 70%; width: 12px; height: 12px; background: #ffc107; border-radius: 50%; box-shadow: 0 0 10px #ffc107;"></div>
    </div>
  </div>

  <div class="logs-panel">
    <div class="table-header">
      <h3>Recent Maintenance Logs</h3>
      <span class="view-all">View All Logs</span>
    </div>
    <table>
      <thead>
        <tr>
          <th width="15%">ASSET ID</th>
          <th width="30%">SERVICE TYPE</th>
          <th width="20%">STATUS</th>
          <th width="20%">TECHNICIAN</th>
          <th width="15%">DATE</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(log, index) in store.logs" :key="index">
          <td class="fw-bold" style="color: #1d84d1;">{{ log.assetId }}</td>
          <td>{{ log.serviceType }}</td>
          <td>
            <span class="status-badge" :class="parseStatusClass(log.status)">
              {{ log.status }}
            </span>
          </td>
          <td>{{ log.technician }}</td>
          <td style="color: var(--text-muted); font-size: 0.85rem;">{{ log.date }}</td>
        </tr>
        <tr v-if="store.logs.length === 0">
          <td colspan="5" style="text-align: center; color: var(--text-muted);">No recent logs.</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style scoped>
@keyframes slideIn {
  from { opacity: 0; transform: translateX(-20px); }
  to { opacity: 1; transform: translateX(0); }
}
.animate-slide-in {
  animation: slideIn 0.4s ease forwards;
}
</style>
