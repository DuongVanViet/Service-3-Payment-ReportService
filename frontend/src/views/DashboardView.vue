<template>
  <div class="min-h-screen bg-gray-100">
    <!-- Header -->
    <div class="bg-gradient-to-r from-blue-600 to-blue-800 text-white p-6">
      <div class="container mx-auto flex justify-between items-center">
        <div>
          <h1 class="text-3xl font-bold">Dashboard</h1>
          <p class="text-blue-100">Xin chào, {{ user?.fullName }}!</p>
        </div>
        <div class="flex items-center gap-4">
          <div class="text-right">
            <p class="text-sm">{{ user?.email }}</p>
            <p class="text-xs text-blue-100">{{ getRoleName(user?.role) }}</p>
          </div>
          <button @click="logout" class="bg-red-600 hover:bg-red-700 px-4 py-2 rounded-md transition-colors">
            Đăng Xuất
          </button>
        </div>
      </div>
    </div>

    <!-- Main Content -->
    <div class="container mx-auto p-6">
      <!-- Must Change Password Alert -->
      <div v-if="user?.mustChangePassword" class="mb-6 bg-orange-50 border-l-4 border-orange-500 p-4 rounded-lg">
        <div class="flex">
          <div class="flex-shrink-0">
            <p class="text-2xl">⚠️</p>
          </div>
          <div class="ml-3">
            <p class="text-sm font-medium text-orange-800">
              Bạn cần phải đổi mật khẩu trước khi tiếp tục
            </p>
            <router-link to="/change-password" class="mt-2 inline-block text-orange-700 hover:text-orange-900 font-semibold">
              Đổi mật khẩu ngay →
            </router-link>
          </div>
        </div>
      </div>

      <!-- Admin Dashboard -->
      <div v-if="user?.role === 'Admin'" class="space-y-6">
        <h2 class="text-2xl font-bold text-gray-900">Quản Lý Hệ Thống</h2>
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          <router-link to="/admin/dashboard" class="block bg-white p-6 rounded-lg shadow hover:shadow-lg transition-shadow hover:border-blue-500 border-l-4 border-blue-500">
            <div class="text-3xl mb-2">📊</div>
            <h3 class="font-bold text-lg text-gray-900">Tổng Quan</h3>
            <p class="text-sm text-gray-600 mt-1">Dashboard doanh thu & nợ</p>
          </router-link>
          <router-link to="/admin/users" class="block bg-white p-6 rounded-lg shadow hover:shadow-lg transition-shadow hover:border-green-500 border-l-4 border-green-500">
            <div class="text-3xl mb-2">👥</div>
            <h3 class="font-bold text-lg text-gray-900">Tài Khoản</h3>
            <p class="text-sm text-gray-600 mt-1">Quản lý người dùng</p>
          </router-link>
          <router-link to="/admin/invoices" class="block bg-white p-6 rounded-lg shadow hover:shadow-lg transition-shadow hover:border-orange-500 border-l-4 border-orange-500">
            <div class="text-3xl mb-2">📄</div>
            <h3 class="font-bold text-lg text-gray-900">Hóa Đơn</h3>
            <p class="text-sm text-gray-600 mt-1">Quản lý học phí</p>
          </router-link>
          <router-link to="/admin/debts" class="block bg-white p-6 rounded-lg shadow hover:shadow-lg transition-shadow hover:border-red-500 border-l-4 border-red-500">
            <div class="text-3xl mb-2">💰</div>
            <h3 class="font-bold text-lg text-gray-900">Công Nợ</h3>
            <p class="text-sm text-gray-600 mt-1">Theo dõi nợ tiền</p>
          </router-link>
        </div>
      </div>

      <!-- Teacher Dashboard -->
      <div v-else-if="user?.role === 'Teacher'" class="space-y-6">
        <h2 class="text-2xl font-bold text-gray-900">Quản Lý Lớp</h2>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <router-link to="/teacher/dashboard" class="block bg-white p-6 rounded-lg shadow hover:shadow-lg transition-shadow hover:border-blue-500 border-l-4 border-blue-500">
            <div class="text-3xl mb-2">📚</div>
            <h3 class="font-bold text-lg text-gray-900">Các Lớp Của Tôi</h3>
            <p class="text-sm text-gray-600 mt-1">Xem danh sách lớp & báo cáo</p>
          </router-link>
          <div class="bg-white p-6 rounded-lg shadow border-l-4 border-gray-300">
            <div class="text-3xl mb-2">📊</div>
            <h3 class="font-bold text-lg text-gray-900">Báo Cáo</h3>
            <p class="text-sm text-gray-600 mt-1">Xem chi tiết từng lớp</p>
          </div>
        </div>
      </div>

      <!-- Student Dashboard -->
      <div v-else-if="user?.role === 'Student'" class="space-y-6">
        <h2 class="text-2xl font-bold text-gray-900">Thông Tin Cá Nhân</h2>
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <router-link to="/student/invoices" class="block bg-white p-6 rounded-lg shadow hover:shadow-lg transition-shadow hover:border-blue-500 border-l-4 border-blue-500">
            <div class="text-3xl mb-2">📄</div>
            <h3 class="font-bold text-lg text-gray-900">Học Phí</h3>
            <p class="text-sm text-gray-600 mt-1">Xem hóa đơn & chi tiết</p>
          </router-link>
          <router-link to="/student/debt" class="block bg-white p-6 rounded-lg shadow hover:shadow-lg transition-shadow hover:border-red-500 border-l-4 border-red-500">
            <div class="text-3xl mb-2">⚠️</div>
            <h3 class="font-bold text-lg text-gray-900">Công Nợ</h3>
            <p class="text-sm text-gray-600 mt-1">Xem nợ còn lại</p>
          </router-link>
          <router-link to="/student/payments" class="block bg-white p-6 rounded-lg shadow hover:shadow-lg transition-shadow hover:border-green-500 border-l-4 border-green-500">
            <div class="text-3xl mb-2">✅</div>
            <h3 class="font-bold text-lg text-gray-900">Thanh Toán</h3>
            <p class="text-sm text-gray-600 mt-1">Lịch sử giao dịch</p>
          </router-link>
        </div>

        <!-- Student Stats -->
        <div class="mt-8">
          <h2 class="text-2xl font-bold text-gray-900 mb-4">Thống Kê Của Tôi</h2>
          <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div class="bg-white p-6 rounded-lg shadow">
              <p class="text-gray-600 text-sm font-medium">Tổng Công Nợ</p>
              <p class="text-3xl font-bold text-red-600 mt-2">{{ formatCurrency(stats.totalDebt) }}</p>
            </div>
            <div class="bg-white p-6 rounded-lg shadow">
              <p class="text-gray-600 text-sm font-medium">Số Hóa Đơn</p>
              <p class="text-3xl font-bold text-blue-600 mt-2">{{ stats.invoiceCount }}</p>
            </div>
            <div class="bg-white p-6 rounded-lg shadow">
              <p class="text-gray-600 text-sm font-medium">Quá Hạn</p>
              <p class="text-3xl font-bold text-orange-600 mt-2">{{ stats.overdueCount }}</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/authStore';
import api from '../api';

const authStore = useAuthStore();
const router = useRouter();

const user = ref(authStore.user);

const stats = ref({
  totalDebt: 0,
  invoiceCount: 0,
  overdueCount: 0
});

const loadStats = async () => {
  if (user.value?.role === 'Student') {
    try {
      const response = await api.get('/student/dashboard-summary');
      stats.value = response.data;
    } catch (error) {
      console.error('Error loading stats:', error);
    }
  }
};

const logout = async () => {
  await authStore.logout();
  router.push('/login');
};

const getRoleName = (role?: string) => {
  const roles: { [key: string]: string } = {
    'Admin': 'Quản Trị Viên',
    'Teacher': 'Giáo Viên',
    'Student': 'Học Viên'
  };
  return roles[role || ''] || role || '';
};

const formatCurrency = (value: number) => {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
};

onMounted(() => {
  loadStats();
});
</script>
