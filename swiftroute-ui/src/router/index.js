import { createRouter, createWebHistory } from 'vue-router'
import DashboardView from '../views/DashboardView.vue'
import MaintenanceView from '../views/MaintenanceView.vue'
import DriversView from '../views/DriversView.vue'
import FleetView from '../views/FleetView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: DashboardView,
    },
    {
      path: '/maintenance',
      name: 'maintenance',
      component: MaintenanceView,
    },
    {
      path: '/drivers',
      name: 'drivers',
      component: DriversView,
    },
    {
      path: '/fleet',
      name: 'fleet',
      component: FleetView,
    }
  ],
})

export default router
