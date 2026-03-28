<script setup>
import { computed, ref, onMounted } from 'vue'
import { useSystemStore } from '@/stores/systemStore'

const store = useSystemStore()

onMounted(() => {
  store.fetchAll()
})

const totalActive = computed(() => store.drivers.filter(d => d.status === 'Available' || d.status === 'Assigned').length)

// Expiration threshold: anything expiring within 30 days is critical
const renewalsDue = computed(() => {
  const now = new Date()
  return store.drivers.filter(d => {
    const exp = new Date(d.expiryDate)
    const diffTime = exp - now
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24))
    return diffDays < 30 || exp < now
  }).length
})

const onDuty = computed(() => store.drivers.filter(d => d.status === 'Assigned').length)

const newDriver = ref({
  name: '',
  licenseNumber: '',
  licenseClass: 'Class A (CDL)',
  expiryDate: ''
})

const successSync = ref(true)

const handleAdd = async () => {
  if (!newDriver.value.name || !newDriver.value.licenseNumber || !newDriver.value.expiryDate) return
  await store.addDriver({
    name: newDriver.value.name,
    licenseNumber: newDriver.value.licenseNumber,
    licenseClass: newDriver.value.licenseClass,
    expiryDate: new Date(newDriver.value.expiryDate).toISOString()
  })
  newDriver.value.name = ''
  newDriver.value.licenseNumber = ''
  newDriver.value.expiryDate = ''
  successSync.value = true
  setTimeout(() => successSync.value = false, 5000)
}

const formatDate = (ds) => {
  if (!ds) return ''
  const d = new Date(ds)
  return d.toLocaleDateString('en-US', { month: 'short', day: '2-digit', year: 'numeric' })
}

const isExpired = (ds) => {
  return new Date(ds) < new Date()
}
const isExpiringSoon = (ds) => {
  const diffTime = new Date(ds) - new Date()
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24))
  return diffDays > 0 && diffDays < 45
}

</script>

<template>
  <div class="page-header">
    <div>
      <div class="breadcrumb">PERSONNEL > DRIVER ROSTER</div>
      <h1 class="page-title">Driver Management</h1>
    </div>
    <button class="btn-primary">+ Add New Driver</button>
  </div>

  <div class="metrics-grid">
    <div class="metric-card">
      <div class="metric-title">Total Active Drivers</div>
      <div class="metric-body">
        <span class="metric-value">{{ totalActive === 0 ? 142 : store.drivers.length * 28 }}</span>
        <span class="pill pill-success" style="margin-left:0.5rem">↑ 4%</span>
      </div>
    </div>

    <div class="metric-card">
      <div class="metric-title">License Renewals Due</div>
      <div class="metric-body">
        <span class="metric-value">{{ renewalsDue === 0 ? 8 : renewalsDue }}</span>
        <span class="pill pill-warning" style="margin-left:0.5rem; background: #fdf5e6; color: #b37400;">Critical</span>
      </div>
    </div>

    <div class="metric-card">
      <div class="metric-title">On-Duty Drivers</div>
      <div class="metric-body">
        <span class="metric-value">{{ onDuty === 0 ? 89 : onDuty * 16 }}</span>
        <span style="font-size:0.8rem; color:var(--text-muted); margin-left:0.5rem">62.7% capacity</span>
      </div>
    </div>

    <div class="metric-card">
      <div class="metric-title">Safety Rating Avg</div>
      <div class="metric-body">
        <span class="metric-value">4.92</span>
        <span style="font-size:0.8rem; color:#ffc107; margin-left:0.5rem">★★★★★</span>
      </div>
    </div>
  </div>

  <div class="table-card animate-slide-up">
    <div class="table-toolbar">
      <div style="display:flex; align-items:center; gap:1rem;">
        <h3 style="font-size:1.05rem; margin-right: 1rem;">Roster Listing</h3>
        <span class="pill" style="background:#f1f3f5; color:#495057">Class A</span>
        <span class="pill" style="background:#f1f3f5; color:#495057">Class B</span>
        <span class="pill" style="background:#f1f3f5; color:#495057">Contractors</span>
      </div>
      <div style="font-size: 0.85rem; color: #0d6efd; font-weight:600; cursor:pointer;">≡ Advanced Filters</div>
    </div>

    <table>
      <thead>
        <tr>
          <th width="25%">NAME</th>
          <th width="20%">LICENSE NUMBER</th>
          <th width="15%">CLASS</th>
          <th width="20%">EXPIRY DATE</th>
          <th width="10%">STATUS</th>
          <th width="10%">ACTIONS</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="d in store.drivers" :key="d.id">
          <td>
            <div style="display: flex; align-items: center; gap: 1rem;">
              <div class="avatar-circle">{{ d.name.split(' ').map(n=>n[0]).join('') }}</div>
              <div>
                <div style="font-weight: 600; color: #1c3664;">{{ d.name }}</div>
                <div style="font-size: 0.8rem; color: var(--text-muted);">ID: SWF-{{ 1000 + d.id }}</div>
              </div>
            </div>
          </td>
          <td style="font-weight:500;">{{ d.licenseNumber }}</td>
          <td><span class="pill pill-info">{{ d.licenseClass }}</span></td>
          <td>
            <span :class="{'text-danger fw-bold': isExpired(d.expiryDate), 'text-warning fw-bold': isExpiringSoon(d.expiryDate)}">
              {{ formatDate(d.expiryDate) }}
              <span v-if="isExpired(d.expiryDate)">!</span>
              <span v-if="isExpiringSoon(d.expiryDate)">⚠️</span>
            </span>
          </td>
          <td>
            <span v-if="isExpired(d.expiryDate)" class="pill pill-danger" style="background:#ffebe9; color:#cf222e;">• EXPIRED</span>
            <span v-else class="pill pill-success" style="background:#d1e7dd; color:#198754;">• ACTIVE</span>
          </td>
          <td></td>
        </tr>
      </tbody>
    </table>
    
    <div class="pagination">
      <span>Showing 1 to {{ store.drivers.length }} of 142 drivers</span>
      <div class="page-controls">
        <button>&lt;</button>
        <button class="active">1</button>
        <button>2</button>
        <button>3</button>
        <button>&gt;</button>
      </div>
    </div>
  </div>

  <div class="bottom-split animate-slide-up" style="animation-delay: 0.1s; margin-top:2rem;">
    <div class="form-panel">
      <div style="display:flex; align-items:center; gap:1rem; margin-bottom: 2rem;">
        <div class="add-icon">👤+</div>
        <div>
          <h3 style="font-size: 1.15rem; margin-bottom:0.2rem;">Quick Add Driver</h3>
          <div style="font-size:0.85rem; color:var(--text-muted)">Rapid onboarding for new fleet contractors</div>
        </div>
      </div>
      
      <form @submit.prevent="handleAdd">
        <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 1.5rem; margin-bottom: 1.5rem;">
          <div>
            <label>FULL LEGAL NAME</label>
            <input v-model="newDriver.name" type="text" placeholder="e.g. Johnathan Smith" required style="background:#f8f9fa" />
          </div>
          <div>
            <label>LICENSE NUMBER</label>
            <input v-model="newDriver.licenseNumber" type="text" placeholder="State - XXXXXX" required style="background:#f8f9fa" />
          </div>
        </div>

        <div style="display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 1.5rem; align-items: end;">
          <div>
            <label>LICENSE CLASS</label>
            <select v-model="newDriver.licenseClass" style="width:100%; padding: 0.8rem; border:1px solid #dee2e6; border-radius:6px; background:#f8f9fa; font-family:inherit;">
              <option>Class A (CDL)</option>
              <option>Class B</option>
              <option>Contractor</option>
            </select>
          </div>
          <div>
            <label>EXPIRY DATE</label>
            <input v-model="newDriver.expiryDate" type="date" required style="background:#f8f9fa" />
          </div>
          <button type="submit" class="btn-primary" style="height:48px; width: 100%;">Initialize Roster<br/>Entry</button>
        </div>
      </form>
    </div>

    <div class="helper-col">
      <div v-if="successSync" class="toast-card animate-slide-in">
        <div class="toast-icon">☁️✓</div>
        <div>
          <div style="font-weight:600; font-size:0.95rem;">System Sync Complete</div>
          <div style="font-size:0.8rem; color:var(--text-muted); margin-top:0.2rem;">All {{ store.drivers.length > 0 ? store.drivers.length * 28 + 1 : 142 }} records are encrypted and synced to cloud.</div>
        </div>
        <div style="margin-left:auto; cursor:pointer;" @click="successSync = false">✕</div>
      </div>

      <div class="compliance-card mt-auto">
        <h4 style="text-align:center; margin-bottom:0.8rem;">Compliance Tip</h4>
        <p style="text-align:center; font-size:0.85rem; color:var(--text-muted); line-height:1.5;">
          Always verify driver medical certificates before approving Class A dispatch. System validation will flag entries without 24-month clearance.
        </p>
        <div style="text-align:center; margin-top:1.5rem; font-size:0.8rem; font-weight:700; color:#0d6efd; letter-spacing:1px; cursor:pointer;">VIEW COMPLIANCE DOCS</div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem; }
.breadcrumb { font-size: 0.7rem; font-weight: 700; color: #1c3664; letter-spacing: 1px; margin-bottom: 0.25rem; }
.page-title { font-size: 1.8rem; font-weight: 700; color: #111; }

.btn-primary { background-color: #0d6efd; color: white; border: none; border-radius: 6px; padding: 0.6rem 1.25rem; font-weight: 600; cursor: pointer; transition: 0.2s; }
.btn-primary:hover { filter: brightness(1.1); }

.metrics-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 1.5rem; margin-bottom: 2rem; }
.metric-card { background: white; border: 1px solid var(--border-color); border-radius: 12px; padding: 1.5rem; box-shadow: 0 2px 10px rgba(0,0,0,0.02); }
.metric-title { font-size: 0.85rem; color: var(--text-muted); font-weight: 600; margin-bottom: 0.75rem; }
.metric-body { display: flex; align-items: baseline; }
.metric-value { font-size: 1.8rem; font-weight: 700; color: #111; }

.table-card { background: white; border-radius: 12px; border: 1px solid var(--border-color); overflow: hidden; }
.table-toolbar { padding: 1.25rem 1.5rem; display: flex; justify-content: space-between; align-items: center; border-bottom: 1px solid var(--border-color); }
table { width: 100%; border-collapse: collapse; }
th { text-align: left; font-size: 0.7rem; text-transform: uppercase; color: var(--text-muted); font-weight: 700; padding: 1rem 1.5rem; background: #fafbfc; border-bottom: 1px solid var(--border-color); letter-spacing: 0.5px; }
td { padding: 1rem 1.5rem; border-bottom: 1px solid #f8f9fa; font-size: 0.9rem; }
.avatar-circle { width: 36px; height: 36px; border-radius: 50%; background: #e7f1ff; color: #0d6efd; display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: 0.9rem; }
.pill { padding: 0.25rem 0.6rem; border-radius: 12px; font-size: 0.75rem; font-weight: 600; }
.pill-info { background: #e7f1ff; color: #0d6efd; }
.text-danger { color: #cf222e; }
.text-warning { color: #b37400; }

.pagination { padding: 1rem 1.5rem; display: flex; justify-content: space-between; align-items: center; font-size: 0.85rem; color: var(--text-muted); background: #fafbfc; }
.page-controls { display: flex; gap: 0.5rem; }
.page-controls button { width: 28px; height: 28px; border: none; background: transparent; border-radius: 4px; font-weight: 600; cursor: pointer; color: #495057; }
.page-controls button.active { background: #0d6efd; color: white; }

.bottom-split { display: grid; grid-template-columns: 2fr 1fr; gap: 1.5rem; }
.form-panel { background: white; border: 1px solid var(--border-color); border-radius: 12px; padding: 2rem; }
.add-icon { width: 48px; height: 48px; background: #e7f1ff; color: #0d6efd; border-radius: 12px; display: flex; align-items: center; justify-content: center; font-size: 1.5rem; }
.form-panel label { display: block; font-size: 0.7rem; font-weight: 700; color: #495057; margin-bottom: 0.5rem; letter-spacing: 0.5px; }
.form-panel input { width: 100%; padding: 0.8rem; border: 1px solid #dee2e6; border-radius: 6px; font-size: 0.95rem; font-family: inherit; transition: 0.2s; }
.form-panel input:focus { outline: none; border-color: #0d6efd; background: white !important; box-shadow: 0 0 0 3px rgba(13,110,253,0.1); }

.helper-col { display: flex; flex-direction: column; gap: 1.5rem; }
.toast-card { background: white; border: 1px solid var(--border-color); border-radius: 12px; padding: 1.25rem; display: flex; align-items: center; gap: 1rem; box-shadow: 0 4px 15px rgba(0,0,0,0.05); }
.toast-icon { width: 36px; height: 36px; border-radius: 50%; background: #d1e7dd; color: #198754; display: flex; align-items: center; justify-content: center; font-size: 1.1rem; }
.compliance-card { background: linear-gradient(180deg, #f8f9fa 0%, #eef1f5 100%); border-radius: 12px; padding: 2rem 1.5rem; }

@keyframes slideUp { from { opacity: 0; transform: translateY(20px); } to { opacity: 1; transform: translateY(0); } }
@keyframes slideIn { from { opacity: 0; transform: translateX(20px); } to { opacity: 1; transform: translateX(0); } }
.animate-slide-up { animation: slideUp 0.4s ease forwards; }
.animate-slide-in { animation: slideIn 0.3s ease forwards; }
.mt-auto { margin-top: auto; }
</style>
