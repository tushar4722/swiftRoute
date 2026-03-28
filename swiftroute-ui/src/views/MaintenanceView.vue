<script setup>
import { computed, ref, onMounted } from 'vue'
import { useSystemStore } from '@/stores/systemStore'

const store = useSystemStore()

onMounted(() => {
  store.fetchAll()
  store.initSignalR()
})

const totalFleet = computed(() => store.vehicles.length)
const maintenanceRequired = computed(() => store.vehicles.filter(v => v.status === 'Maintenance Required').length)
const activeHealthy = computed(() => store.vehicles.filter(v => v.status === 'Active').length)
const outOfService = computed(() => store.vehicles.filter(v => !['Active', 'Maintenance Required'].includes(v.status)).length)

const newReg = ref({
  plateNumber: '',
  model: '',
  year: new Date().getFullYear(),
  currentOdometer: 0
})

const handleRegister = async () => {
  if (!newReg.value.plateNumber || !newReg.value.model) return
  await store.registerVehicle({
    plateNumber: newReg.value.plateNumber,
    model: newReg.value.model,
    year: parseInt(newReg.value.year),
    currentOdometer: parseInt(newReg.value.currentOdometer)
  })
  // reset
  newReg.value.plateNumber = ''
  newReg.value.model = ''
  newReg.value.currentOdometer = 0
}

const formatOdo = (val) => val?.toLocaleString() + ' km'

</script>

<template>
  <div class="page-header">
    <div>
      <h1 class="page-title">Maintenance Hub</h1>
      <p class="page-subtitle">Monitor health and service schedules for the SwiftRoute fleet.</p>
    </div>
    <button class="btn-primary" style="padding: 0.6rem 1.25rem;">+ Add Vehicle</button>
  </div>

  <div class="metrics-grid">
    <div class="metric-card animate-slide-up" style="animation-delay: 0.1s">
      <div class="metric-header">
        <div class="icon-box icon-blue">🚛</div>
        <span class="pill pill-success">+12%</span>
      </div>
      <div class="metric-value">{{ totalFleet }}</div>
      <div class="metric-title">Total Fleet</div>
    </div>

    <div class="metric-card animate-slide-up" style="animation-delay: 0.2s">
      <div class="metric-header">
        <div class="icon-box icon-warning" style="background: #fdf5e6; color: #b37400;">⚠️</div>
        <span class="pill" style="background: #fdf5e6; color: #b37400;">Action Required</span>
      </div>
      <div class="metric-value">{{ maintenanceRequired }}</div>
      <div class="metric-title">Maintenance Required</div>
    </div>

    <div class="metric-card animate-slide-up" style="animation-delay: 0.3s">
      <div class="metric-header">
        <div class="icon-box icon-blue" style="background: #f0f7ff; color: #1c3664;">✓</div>
      </div>
      <div class="metric-value">{{ activeHealthy }}</div>
      <div class="metric-title">Active & Healthy</div>
    </div>

    <div class="metric-card animate-slide-up" style="animation-delay: 0.4s">
      <div class="metric-header">
        <div class="icon-box icon-danger" style="background: #ffebe9; color: #cf222e;">✖</div>
      </div>
      <div class="metric-value">{{ outOfService }}</div>
      <div class="metric-title">Out of Service</div>
    </div>
  </div>

  <div class="logs-panel mb-4 animate-slide-in">
    <div class="table-header">
      <h3 style="font-size: 1.1rem;">Vehicle Health Overview</h3>
      <div class="toggle-group">
        <button class="toggle-btn active">All Vehicles</button>
        <button class="toggle-btn">By Priority</button>
      </div>
    </div>

    <table>
      <thead>
        <tr>
          <th width="35%">VEHICLE IDENTITY</th>
          <th width="20%">STATUS</th>
          <th width="25%">ODOMETER</th>
          <th width="20%" style="text-align: right;">ACTION</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="v in store.vehicles" :key="v.id">
          <td>
            <div style="display: flex; align-items: center; gap: 1rem;">
              <div style="width: 40px; height: 40px; border-radius: 8px; background: #f1f3f5; display: flex; align-items: center; justify-content: center; font-size: 1.2rem;">🚛</div>
              <div>
                <div style="font-weight: 600; color: #1c3664;">{{ v.plateNumber }}</div>
                <div style="font-size: 0.8rem; color: var(--text-muted);">{{ v.year }} {{ v.model }}</div>
              </div>
            </div>
          </td>
          <td>
            <span class="status-badge" 
                  :class="{'warning': v.status === 'Maintenance Required', 'progress': v.status === 'In-Transit' || v.status === 'Assigned'}">
              {{ v.status }}
            </span>
          </td>
          <td>
            <div style="font-weight: 700; font-size: 0.95rem; margin-bottom: 0.25rem;">{{ formatOdo(v.currentOdometer) }}</div>
            <div style="width: 100px; height: 4px; background: #e9ecef; border-radius: 2px; overflow: hidden;">
              <div :style="{ width: Math.min(100, ((v.currentOdometer - v.lastServiceOdometer) / 10000) * 100) + '%', background: (v.currentOdometer - v.lastServiceOdometer) >= 10000 ? '#b37400' : '#0d6efd', height: '100%' }"></div>
            </div>
          </td>
          <td style="text-align: right;">
            <button v-if="v.status === 'Maintenance Required'" class="btn-primary" style="padding: 0.4rem 1rem; font-size: 0.8rem;" @click="store.serviceVehicle(v.id)">Service Vehicle</button>
            <span v-else style="color: #0d6efd; font-weight: 600; font-size: 0.85rem; cursor: pointer;">Details</span>
          </td>
        </tr>
      </tbody>
    </table>
  </div>

  <div class="bottom-split animate-slide-up" style="animation-delay: 0.2s">
    <div class="form-panel">
      <h3 style="margin-bottom: 1.5rem; font-size: 1.1rem;">Quick Registration</h3>
      <form @submit.prevent="handleRegister">
        <label>Plate Number</label>
        <input v-model="newReg.plateNumber" type="text" placeholder="e.g. SR-0000-XX" required />
        
        <div style="display: grid; grid-template-columns: 2fr 1fr; gap: 1rem; margin-top: 1rem;">
          <div>
            <label>Make/Model</label>
            <input v-model="newReg.model" type="text" placeholder="Make" required />
          </div>
          <div>
            <label>Year</label>
            <input v-model="newReg.year" type="number" placeholder="2024" required />
          </div>
        </div>

        <div style="margin-top: 1rem;">
          <label>Initial Odometer</label>
          <input v-model="newReg.currentOdometer" type="number" placeholder="km" required />
        </div>

        <button type="submit" class="btn-primary" style="width: 100%; margin-top: 1.5rem; background: #1c3664; box-shadow: none; font-size: 0.95rem;">Register Vehicle</button>
      </form>
    </div>

    <div class="history-col">
      <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 1rem;">
        <h3 style="font-size: 1.1rem;">Maintenance History</h3>
        <span style="color: #0d6efd; font-weight: 600; font-size: 0.85rem; cursor: pointer;">View All Records</span>
      </div>

      <div class="history-card" v-for="(log, idx) in store.logs.slice(0, 3)" :key="idx">
        <div class="hist-icon">⚙️</div>
        <div class="hist-body">
          <div style="display: flex; justify-content: space-between;">
            <div class="hist-title">{{ log.serviceType }}</div>
            <div class="hist-date">{{ log.date }}</div>
          </div>
          <div class="hist-sub">Vehicle: <strong>{{ log.assetId }}</strong></div>
          <div class="hist-meta">
             <span style="color: var(--success);">✓ {{ log.status }}</span> &nbsp;&nbsp; Tech: {{ log.technician }}
          </div>
        </div>
      </div>

      <div class="predictive-card mt-auto">
        <div class="pred-icon">📊</div>
        <div>
          <div style="font-weight: 700; color: #1c3664; font-size: 1rem;">Predictive Analysis Active</div>
          <div style="color: var(--text-muted); font-size: 0.85rem; margin-top: 0.2rem;">Based on route intensity, 3 more vehicles will reach service thresholds within 7 days.</div>
        </div>
        <button class="btn-outline" style="margin-left: auto;">Download Report</button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: flex-end; margin-bottom: 2rem; }
.page-title { font-size: 1.75rem; font-weight: 700; }
.page-subtitle { color: var(--text-muted); font-size: 0.9rem; margin-top: 0.25rem; }

.btn-primary { background-color: #0d6efd; color: white; border: none; border-radius: var(--radius-sm); font-weight: 600; cursor: pointer; transition: 0.2s; box-shadow: 0 4px 10px rgba(13, 110, 253, 0.2); }
.btn-primary:hover { transform: translateY(-1px); box-shadow: 0 6px 15px rgba(13, 110, 253, 0.3); }

.metrics-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 1.5rem; margin-bottom: 2rem; }
.metric-card { background-color: var(--surface-color); border: 1px solid var(--border-color); border-radius: var(--radius-md); padding: 1.5rem; box-shadow: 0 2px 10px rgba(0,0,0,0.02); }
.metric-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 1rem; }
.icon-box { width: 40px; height: 40px; border-radius: var(--radius-sm); display: flex; align-items: center; justify-content: center; font-size: 1.25rem; }
.icon-blue { background-color: #f0f7ff; color: #0d6efd; }
.pill { padding: 0.2rem 0.6rem; border-radius: 12px; font-size: 0.75rem; font-weight: 600; }
.pill-success { background-color: #d1e7dd; color: #198754; }
.metric-value { font-size: 1.75rem; font-weight: 700; color: #1c3664; margin-bottom: 0.25rem; }
.metric-title { color: var(--text-muted); font-size: 0.85rem; font-weight: 500; }

.mb-4 { margin-bottom: 2rem; }

.logs-panel { background-color: var(--surface-color); border: 1px solid var(--border-color); border-radius: var(--radius-md); padding: 1.5rem; box-shadow: 0 2px 10px rgba(0,0,0,0.02); }
.table-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
.toggle-group { display: flex; background: #f1f3f5; border-radius: 6px; padding: 0.25rem; }
.toggle-btn { border: none; background: transparent; padding: 0.4rem 1rem; font-size: 0.8rem; font-weight: 600; color: var(--text-muted); border-radius: 4px; cursor: pointer; }
.toggle-btn.active { background: white; color: #1c3664; box-shadow: 0 1px 3px rgba(0,0,0,0.1); }

table { width: 100%; border-collapse: collapse; }
th { text-align: left; font-size: 0.7rem; text-transform: uppercase; color: var(--text-muted); font-weight: 700; padding-bottom: 1rem; border-bottom: 1px solid var(--border-color); letter-spacing: 0.5px; }
td { padding: 1.25rem 0; border-bottom: 1px solid #f8f9fa; }

.status-badge { padding: 0.3rem 0.8rem; border-radius: 12px; font-size: 0.75rem; font-weight: 600; background-color: #d1e7dd; color: #198754; }
.status-badge.warning { background-color: #fdf5e6; color: #b37400; }
.status-badge.progress { background-color: #e7f1ff; color: #0d6efd; }

.bottom-split { display: grid; grid-template-columns: 1fr 2fr; gap: 1.5rem; }
.form-panel { background-color: var(--surface-color); border: 1px solid var(--border-color); border-radius: var(--radius-md); padding: 1.5rem; height: 100%; }
.form-panel label { display: block; font-size: 0.75rem; font-weight: 600; color: #1c3664; margin-bottom: 0.5rem; text-transform: uppercase; }
.form-panel input { width: 100%; padding: 0.75rem; border: 1px solid #dee2e6; border-radius: 6px; font-size: 0.95rem; font-family: inherit; }
.form-panel input:focus { outline: none; border-color: #0d6efd; }

.history-col { display: flex; flex-direction: column; gap: 1rem; }
.history-card { background-color: var(--surface-color); border: 1px solid var(--border-color); border-radius: var(--radius-md); padding: 1.25rem; display: flex; gap: 1rem; }
.hist-icon { width: 44px; height: 44px; display: flex; align-items: center; justify-content: center; background: #fdf5e6; border-radius: 8px; font-size: 1.5rem; }
.hist-body { flex: 1; }
.hist-title { font-weight: 700; font-size: 1.05rem; color: #1c3664; }
.hist-date { font-size: 0.75rem; font-weight: 700; color: var(--text-muted); background: #f1f3f5; padding: 0.2rem 0.6rem; border-radius: 4px; }
.hist-sub { font-size: 0.85rem; color: var(--text-muted); margin: 0.3rem 0; }
.hist-meta { font-size: 0.75rem; font-weight: 600; color: var(--text-muted); margin-top: 0.5rem; }

.predictive-card { background: #f8f9fa; border: 1px dashed #dee2e6; border-radius: var(--radius-md); padding: 1.5rem; display: flex; align-items: center; gap: 1rem; margin-top: 1rem; }
.pred-icon { width: 48px; height: 48px; background: #0d6efd; color: white; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 1.5rem; }
.btn-outline { background: #e9ecef; border: none; padding: 0.6rem 1rem; border-radius: var(--radius-sm); font-weight: 600; font-size: 0.85rem; cursor: pointer; }

@keyframes slideIn { from { opacity: 0; transform: translateX(-20px); } to { opacity: 1; transform: translateX(0); } }
@keyframes slideUp { from { opacity: 0; transform: translateY(20px); } to { opacity: 1; transform: translateY(0); } }
.animate-slide-in { animation: slideIn 0.4s ease forwards; }
.animate-slide-up { animation: slideUp 0.4s ease forwards; }
.mt-auto { margin-top: auto; }
</style>
