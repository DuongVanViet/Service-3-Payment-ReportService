import { createRouter, createWebHistory } from 'vue-router';
import LoginView from './views/LoginView.vue';
import DashboardView from './views/DashboardView.vue';
import ChangePasswordView from './views/ChangePasswordView.vue';
import AdminDashboardView from './views/AdminDashboardView.vue';
import AdminUsersView from './views/AdminUsersView.vue';
import AdminInvoicesView from './views/AdminInvoicesView.vue';
import AdminDebtsView from './views/AdminDebtsView.vue';
import StudentInvoicesView from './views/StudentInvoicesView.vue';
import StudentDebtView from './views/StudentDebtView.vue';
import StudentPaymentsView from './views/StudentPaymentsView.vue';
import TeacherDashboardView from './views/TeacherDashboardView.vue';
import { useAuthStore } from './stores/authStore';

const routes = [
  { path: '/', redirect: '/login' },
  { path: '/login', component: LoginView, meta: { requiresGuest: true } },
  { path: '/change-password', component: ChangePasswordView, meta: { requiresAuth: true } },
  
  // Student routes
  { path: '/student/dashboard', component: DashboardView, meta: { requiresAuth: true, requiresRole: 'Student' } },
  { path: '/student/invoices', component: StudentInvoicesView, meta: { requiresAuth: true, requiresRole: 'Student' } },
  { path: '/student/debt', component: StudentDebtView, meta: { requiresAuth: true, requiresRole: 'Student' } },
  { path: '/student/payments', component: StudentPaymentsView, meta: { requiresAuth: true, requiresRole: 'Student' } },
  
  // Teacher routes
  { path: '/teacher/dashboard', component: TeacherDashboardView, meta: { requiresAuth: true, requiresRole: 'Teacher' } },
  
  // Admin routes
  { path: '/admin/dashboard', component: AdminDashboardView, meta: { requiresAuth: true, requiresRole: 'Admin' } },
  { path: '/admin/users', component: AdminUsersView, meta: { requiresAuth: true, requiresRole: 'Admin' } },
  { path: '/admin/invoices', component: AdminInvoicesView, meta: { requiresAuth: true, requiresRole: 'Admin' } },
  { path: '/admin/debts', component: AdminDebtsView, meta: { requiresAuth: true, requiresRole: 'Admin' } },
  
  // Fallback dashboard
  { path: '/dashboard', component: DashboardView, meta: { requiresAuth: true } },
  { path: '/admin', component: AdminDashboardView, meta: { requiresAuth: true, requiresAdmin: true } }
];

const router = createRouter({
  history: createWebHistory(),
  routes
});

// Navigation guards
router.beforeEach((to, from, next) => {
  const authStore = useAuthStore();
  authStore.loadFromStorage();

  const requiresAuth = to.matched.some(record => record.meta.requiresAuth);
  const requiresGuest = to.matched.some(record => record.meta.requiresGuest);
  const requiresAdmin = to.matched.some(record => record.meta.requiresAdmin);

  if (requiresAuth && !authStore.isAuthenticated) {
    // Redirect to login if not authenticated
    next('/login');
  } else if (requiresGuest && authStore.isAuthenticated) {
    // Redirect to dashboard if already authenticated and trying to access login
    if (authStore.mustChangePassword) {
      next('/change-password');
    } else {
      next('/dashboard');
    }
  } else if (requiresAdmin && !authStore.isAdmin) {
    // Redirect if not admin
    next('/dashboard');
  } else if (authStore.isAuthenticated && authStore.mustChangePassword && to.path !== '/change-password') {
    // Force user to change password
    next('/change-password');
  } else {
    next();
  }
});

export default router;
