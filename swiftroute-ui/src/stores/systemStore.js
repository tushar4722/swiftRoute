import { defineStore } from 'pinia'
import * as signalR from '@microsoft/signalr'

const API_BASE = 'http://localhost:5059/api'

export const useSystemStore = defineStore('system', {
  state: () => ({
    drivers: [],
    vehicles: [],
    stats: {
      totalDrivers: 0,
      totalVehicles: 0,
      activeAssignments: 0,
      maintenanceRequired: 0
    },
    logs: [],
    notifications: [],
    activeAssignments: [],
    hubConnection: null,
    loading: false,
    error: null
  }),

  actions: {
    async fetchDrivers() {
      try {
        const res = await fetch(`${API_BASE}/drivers`)
        this.drivers = await res.json()
      } catch (err) { }
    },

    async fetchVehicles() {
      try {
        const res = await fetch(`${API_BASE}/vehicles`)
        this.vehicles = await res.json()
      } catch (err) { }
    },

    async fetchStats() {
      try {
        const res = await fetch(`${API_BASE}/dashboard/stats`)
        this.stats = await res.json()
      } catch (err) { }
    },

    async fetchLogs() {
      try {
        const res = await fetch(`${API_BASE}/dashboard/logs`)
        this.logs = await res.json()
      } catch (err) { }
    },

    async fetchActiveAssignments() {
      try {
        const res = await fetch(`${API_BASE}/assignments/active`)
        this.activeAssignments = await res.json()
      } catch (err) { }
    },

    async fetchAll() {
      await Promise.all([
        this.fetchDrivers(),
        this.fetchVehicles(),
        this.fetchStats(),
        this.fetchLogs(),
        this.fetchActiveAssignments()
      ])
    },

    async initSignalR() {
      if (this.hubConnection) return

      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl('http://localhost:5059/hub/notifications')
        .withAutomaticReconnect()
        .build()

      this.hubConnection.on('ReceiveAlert', (message) => {
        const alertId = Date.now()
        this.notifications.push({ id: alertId, text: message })
        
        // Auto remove after 5 seconds
        setTimeout(() => {
          this.removeNotification(alertId)
        }, 5000)
      })

      this.hubConnection.on('RefreshStats', () => {
        // Triggered whenever there is a database mutation
        setTimeout(() => this.fetchAll(), 300)
      })

      try {
        await this.hubConnection.start()
        console.log('SignalR connected.')
      } catch (err) {
        console.error('SignalR failed', err)
      }
    },

    removeNotification(id) {
      this.notifications = this.notifications.filter(n => n.id !== id)
    },

    async assignDriver(driverId, vehicleId) {
      try {
        this.loading = true
        this.error = null
        const res = await fetch(`${API_BASE}/assignments?driverId=${driverId}&vehicleId=${vehicleId}`, { method: 'POST' })
        const data = await res.text()
        if (!res.ok) throw new Error(data)
        return true
      } catch (err) {
        this.error = err.message
        return false
      } finally {
        this.loading = false
      }
    },

    async serviceVehicle(vehicleId) {
      try {
        await fetch(`${API_BASE}/vehicles/service?vehicleId=${vehicleId}`, { method: 'POST' })
      } catch (err) { }
    },

    async addMiles(vehicleId, currentOdometer) {
      try {
        await fetch(`${API_BASE}/vehicles/${vehicleId}/odometer`, {
          method: 'PUT',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(currentOdometer + 12000)
        })
      } catch (err) { }
    },

    async registerVehicle(payload) {
      try {
        const res = await fetch(`${API_BASE}/vehicles`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(payload)
        })
        if (res.ok) {
          await this.fetchAll()
        }
      } catch (err) { }
    },

    async addDriver(payload) {
      try {
        const res = await fetch(`${API_BASE}/drivers`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(payload)
        })
        if (res.ok) {
          await this.fetchAll()
        }
      } catch (err) { }
    }
  }
})
