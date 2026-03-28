<script setup>
import { computed, ref, onMounted } from 'vue'
import { useSystemStore } from '@/stores/systemStore'

const store = useSystemStore()

onMounted(() => {
  store.fetchAll()
})

const selectedDriverId = ref('')
const selectedVehicleId = ref('')

// Filter arrays to only available entities
const availableDrivers = computed(() => store.drivers.filter(d => ['Available', 'Active'].includes(d.status) && new Date(d.expiryDate) > new Date()))
const availableVehicles = computed(() => store.vehicles.filter(v => v.status === 'Active'))

const selectedDriver = computed(() => store.drivers.find(d => d.id === selectedDriverId.value))
const selectedVehicle = computed(() => store.vehicles.find(v => v.id === selectedVehicleId.value))

const isCompatible = computed(() => {
  if (!selectedDriver.value || !selectedVehicle.value) return false
  return selectedDriver.value.licenseClass.includes(selectedVehicle.value.requiredClass) || selectedDriver.value.licenseClass.includes('CDL') || selectedVehicle.value.requiredClass === 'Class A' // loose mock check
})

const handleAssignment = async () => {
  if (!selectedDriverId.value || !selectedVehicleId.value || !isCompatible.value) return
  const success = await store.assignDriver(selectedDriverId.value, selectedVehicleId.value)
  if (success) {
    selectedDriverId.value = ''
    selectedVehicleId.value = ''
  } else {
    alert("Assignment Failed: " + store.error)
  }
}

const getDuration = (startTime) => {
  const diffTime = new Date() - new Date(startTime)
  const hrs = Math.floor(diffTime / (1000 * 60 * 60))
  const mins = Math.floor((diffTime % (1000 * 60 * 60)) / (1000 * 60))
  return `${String(hrs).padStart(2, '0')}h ${String(mins).padStart(2, '0')}m`
}

</script>

<template>
  <div class="page-header">
    <div>
      <div class="breadcrumb">FLEET MANAGEMENT > ASSIGNMENT SYSTEM</div>
      <h1 class="page-title">Fleet Pairing & Dispatch</h1>
      <p class="page-subtitle">Efficiently pair qualified drivers with available vehicles. Validation checks are automatically performed based on license class and maintenance status.</p>
    </div>
  </div>

  <div class="pairing-grid">
    <!-- Step 1 -->
    <div class="step-card animate-slide-up" style="animation-delay: 0.1s">
      <div class="step-header">
        <h3 style="font-size:1.05rem">1. Select Driver</h3>
        <span class="pill pill-muted">REQUIRED</span>
      </div>
      
      <select v-model="selectedDriverId" class="custom-select">
        <option value="" disabled>Select Driver...</option>
        <option v-for="d in availableDrivers" :key="d.id" :value="d.id">
          {{ d.name }} ({{ d.licenseClass }})
        </option>
      </select>

      <div class="validation-box" :class="{ success: selectedDriverId }">
        <div class="val-icon">{{ selectedDriverId ? '✓' : '•' }}</div>
        <div>
          <div style="font-weight: 600; font-size: 0.9rem;">{{ selectedDriverId ? 'License Valid' : 'Pending Selection' }}</div>
          <div style="font-size: 0.8rem; color: var(--text-muted); margin-top:0.2rem" v-if="selectedDriverId">
            Expires: {{ new Date(selectedDriver.expiryDate).toLocaleDateString('en-US', {month: 'short', year: 'numeric'}) }}
          </div>
        </div>
      </div>

      <div class="step-footer mt-auto" :class="{ 'text-success fw-bold': selectedDriverId }">
        <span class="dot" :class="{ 'bg-success': selectedDriverId }"></span> Available for next 12 hours
      </div>
    </div>

    <!-- Step 2 -->
    <div class="step-card animate-slide-up" style="animation-delay: 0.2s">
      <div class="step-header">
        <h3 style="font-size:1.05rem">2. Select Vehicle</h3>
        <span class="pill pill-muted">REQUIRED</span>
      </div>
      
      <select v-model="selectedVehicleId" class="custom-select" :disabled="!selectedDriverId">
        <option value="" disabled>Select Vehicle...</option>
        <option v-for="v in availableVehicles" :key="v.id" :value="v.id">
          {{ v.model }} ({{ v.plateNumber }})
        </option>
      </select>

      <div class="validation-box" :class="{ success: selectedVehicleId && isCompatible, error: selectedVehicleId && !isCompatible }">
        <div class="val-icon">{{ selectedVehicleId && isCompatible ? '✓' : (selectedVehicleId ? '✕' : '•') }}</div>
        <div>
          <div style="font-weight: 600; font-size: 0.9rem;">{{ selectedVehicleId && isCompatible ? 'Class Matching' : (selectedVehicleId ? 'Class Mismatch' : 'Pending Selection') }}</div>
          <div style="font-size: 0.8rem; color: var(--text-muted); margin-top:0.2rem" v-if="selectedVehicleId && isCompatible">
            Driver qualified for heavy duty
          </div>
          <div style="font-size: 0.8rem; color: #cf222e; margin-top:0.2rem" v-if="selectedVehicleId && !isCompatible">
            Driver lacks {{ selectedVehicle.requiredClass }} certification.
          </div>
        </div>
      </div>

      <div class="step-footer mt-auto" :class="{ 'text-success fw-bold': selectedVehicleId }">
        <span class="dot" :class="{ 'bg-success': selectedVehicleId }"></span> Fueled & Ready (92%)
      </div>
    </div>

    <!-- Step 3 Finalize -->
    <div class="finalize-card animate-slide-in" style="animation-delay: 0.3s">
      <h3 style="font-size: 1.25rem; margin-bottom: 2rem;">Finalize Assignment</h3>
      
      <div class="summary-row">
        <span>Route Complexity</span>
        <span class="fw-bold">Standard</span>
      </div>
      <div class="summary-row">
        <span>Departure Window</span>
        <span class="fw-bold">Within 45m</span>
      </div>
      <div class="summary-row" style="border-bottom:none">
        <span>Priority Level</span>
        <span class="fw-bold">High</span>
      </div>

      <button class="btn-finalize mt-auto" :disabled="!isCompatible" @click="handleAssignment">
        🔗 Complete Pairing
      </button>
    </div>
  </div>

  <div class="table-card animate-slide-up" style="animation-delay: 0.4s; margin-top: 2.5rem; position: relative;">
    <div class="table-toolbar">
      <div>
        <h3 style="font-size:1.15rem; margin-bottom: 0.25rem;">Active Assignments</h3>
        <div style="font-size: 0.8rem; color: var(--text-muted);">Currently active pairings and their route status.</div>
      </div>
      <div style="display:flex; gap: 1rem; align-items:center;">
        <span style="font-size: 0.85rem; font-weight:600; cursor:pointer;">≡ Filter</span>
        <span style="font-size: 0.85rem; font-weight:600; cursor:pointer;">↓ Export</span>
      </div>
    </div>

    <table>
      <thead>
        <tr>
          <th width="25%">DRIVER</th>
          <th width="25%">VEHICLE</th>
          <th width="20%">STATUS</th>
          <th width="20%">DURATION</th>
          <th width="10%">ACTIONS</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="a in store.activeAssignments" :key="a.id">
          <td>
            <div style="display: flex; align-items: center; gap: 1rem;">
              <div class="avatar-circle" style="background: #fdf5e6; color: #b37400;">
                {{ a.driverName.split(' ').map(n=>n[0]).join('') }}
              </div>
              <div>
                <div style="font-weight: 700; color: #111;">{{ a.driverName }}</div>
                <div style="font-size: 0.8rem; color: var(--text-muted);">ID: #DR-{{ 4000 + a.driverId }}</div>
              </div>
            </div>
          </td>
          <td>
            <div style="font-weight: 700; color: #111;">{{ a.vehicleModel }}</div>
            <div style="font-size: 0.8rem; color: var(--text-muted);">License: {{ a.plateNumber }}</div>
          </td>
          <td>
            <span class="status-badge" :class="{
              'success': a.status === 'On Route',
              'info': a.status === 'Staging',
              'muted': a.status === 'Resting'
            }">
              <span class="pulse-dot"></span> {{ a.status || 'On Route' }}
            </span>
          </td>
          <td style="font-weight: 500;">
            {{ getDuration(a.startTime) }}
          </td>
          <td></td>
        </tr>
        <tr v-if="store.activeAssignments.length === 0">
          <td colspan="5" style="text-align: center; color: var(--text-muted); padding: 2rem;">No active assignments.</td>
        </tr>
      </tbody>
    </table>

    <div class="network-widget">
      <div style="font-size: 0.75rem; letter-spacing: 1px; color: rgba(255,255,255,0.7); text-transform: uppercase;">Network Status</div>
      <div style="font-weight: 600; font-size: 0.9rem;">{{ store.activeAssignments.length }} active units in sector</div>
      <div class="net-lines"></div>
    </div>
  </div>

</template>

<style scoped>
.page-header { margin-bottom: 2rem; max-width: 800px; }
.breadcrumb { font-size: 0.7rem; font-weight: 700; color: #495057; letter-spacing: 1px; margin-bottom: 0.5rem; }
.page-title { font-size: 2rem; font-weight: 700; color: #111; margin-bottom: 0.5rem; }
.page-subtitle { color: #6c757d; font-size: 0.95rem; line-height: 1.5; }

.pairing-grid { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 1.5rem; height: 320px; }
.step-card { background: white; border: 1px solid var(--border-color); border-radius: 12px; padding: 1.75rem; display: flex; flex-direction: column; box-shadow: 0 4px 15px rgba(0,0,0,0.02); }
.step-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
.pill { padding: 0.25rem 0.6rem; border-radius: 12px; font-size: 0.7rem; font-weight: 700; letter-spacing: 0.5px; }
.pill-muted { background: #e9ecef; color: #495057; }

.custom-select { width: 100%; padding: 1rem; border: 1px solid #dee2e6; border-radius: 8px; font-size: 1rem; font-family: inherit; background: #f8f9fa; color: #111; cursor: pointer; transition: 0.2s; outline: none; margin-bottom: 1.5rem; -webkit-appearance: none; background-image: url('data:image/svg+xml;utf8,<svg fill="%23495057" height="24" viewBox="0 0 24 24" width="24" xmlns="http://www.w3.org/2000/svg"><path d="M7 10l5 5 5-5z"/></svg>'); background-repeat: no-repeat; background-position-x: 95%; background-position-y: 50%; }
.custom-select:focus { border-color: #0d6efd; background: white; box-shadow: 0 0 0 3px rgba(13,110,253,0.1); }
.custom-select:disabled { opacity: 0.5; cursor: not-allowed; }

.validation-box { background: #f8f9fa; border-radius: 8px; padding: 1rem; display: flex; gap: 1rem; align-items: flex-start; transition: 0.3s; }
.validation-box.success { background: #f0fdf4; color: #15803d; }
.validation-box.error { background: #fef2f2; color: #b91c1c; }
.val-icon { width: 24px; height: 24px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-weight: 700; flex-shrink: 0; background: #e9ecef; color: #6c757d; }
.validation-box.success .val-icon { background: #16a34a; color: white; }
.validation-box.error .val-icon { background: #dc2626; color: white; }

.step-footer { font-size: 0.8rem; color: #6c757d; display: flex; align-items: center; gap: 0.5rem; }
.dot { display: inline-block; width: 6px; height: 6px; border-radius: 50%; background: #ced4da; }
.bg-success { background: #16a34a !important; }

.finalize-card { background: #338fcf; color: white; border-radius: 12px; padding: 2rem; display: flex; flex-direction: column; box-shadow: 0 10px 25px rgba(28, 114, 219, 0.25); position: relative; overflow: hidden; }
.finalize-card::before { content: ''; position: absolute; top: -50px; right: -50px; width: 200px; height: 200px; background: rgba(255,255,255,0.05); border-radius: 50%; }

.summary-row { display: flex; justify-content: space-between; padding: 1rem 0; border-bottom: 1px solid rgba(255,255,255,0.15); font-size: 0.95rem; }
.btn-finalize { background: white; color: #1d6ca1; border: none; padding: 1rem; border-radius: 8px; font-weight: 700; font-size: 1.05rem; cursor: pointer; transition: 0.2s; box-shadow: 0 4px 15px rgba(0,0,0,0.1); }
.btn-finalize:hover:not(:disabled) { transform: translateY(-2px); box-shadow: 0 6px 20px rgba(0,0,0,0.15); }
.btn-finalize:disabled { opacity: 0.7; cursor: not-allowed; }

.table-card { background: white; border-radius: 12px; border: 1px solid var(--border-color); overflow: hidden; }
.table-toolbar { padding: 1.5rem; display: flex; justify-content: space-between; align-items: center; border-bottom: 1px solid var(--border-color); }
table { width: 100%; border-collapse: collapse; }
th { text-align: left; font-size: 0.7rem; text-transform: uppercase; color: var(--text-muted); font-weight: 700; padding: 1rem 1.5rem; background: #fafbfc; border-bottom: 1px solid var(--border-color); letter-spacing: 0.5px; }
td { padding: 1.25rem 1.5rem; border-bottom: 1px solid #f8f9fa; }
.avatar-circle { width: 44px; height: 44px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: 1.1rem; }

.status-badge { padding: 0.35rem 0.8rem; border-radius: 20px; font-size: 0.8rem; font-weight: 600; display: inline-flex; align-items: center; gap: 0.4rem; }
.status-badge.success { background: #dcfce7; color: #15803d; }
.status-badge.info { background: #e0e7ff; color: #4338ca; }
.status-badge.muted { background: #f1f5f9; color: #475569; }
.pulse-dot { width: 6px; height: 6px; border-radius: 50%; background: currentColor; }

.network-widget { position: absolute; bottom: -20px; right: -20px; width: 260px; height: 160px; background: linear-gradient(135deg, #2b6a9e 0%, #1e4b73 100%); color: white; border-top-left-radius: 20px; padding: 1.5rem; box-shadow: -5px -5px 25px rgba(0,0,0,0.1); }
.net-lines { position: absolute; bottom: 10px; right: 10px; width: 150px; height: 80px; background-image: radial-gradient(white 1px, transparent 1px); background-size: 15px 15px; opacity: 0.2; }

@keyframes slideUp { from { opacity: 0; transform: translateY(20px); } to { opacity: 1; transform: translateY(0); } }
@keyframes slideIn { from { opacity: 0; transform: translateX(30px); } to { opacity: 1; transform: translateX(0); } }
.animate-slide-up { animation: slideUp 0.4s ease forwards; }
.animate-slide-in { animation: slideIn 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275) forwards; }
.fw-bold { font-weight: 700; }
.text-success { color: #16a34a; }
.mt-auto { margin-top: auto; }
</style>
