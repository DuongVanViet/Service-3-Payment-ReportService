<template>
  <div class="container mx-auto p-6">
    <div class="mb-6 flex justify-between items-center">
      <div>
        <h1 class="text-3xl font-bold mb-2">Quản Lý Tài Khoản</h1>
        <p class="text-gray-600">Quản lý tất cả tài khoản người dùng trong hệ thống</p>
      </div>
      <button @click="showCreateForm = true" class="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700">
        + Tạo Tài Khoản Mới
      </button>
    </div>

    <!-- Summary -->
    <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-8">
      <div class="bg-blue-50 p-4 rounded-lg border-l-4 border-blue-500">
        <p class="text-gray-600 text-sm">Tổng Tài Khoản</p>
        <p class="text-2xl font-bold text-blue-600">{{ summary.totalUsers }}</p>
      </div>
      <div class="bg-green-50 p-4 rounded-lg border-l-4 border-green-500">
        <p class="text-gray-600 text-sm">Hoạt Động</p>
        <p class="text-2xl font-bold text-green-600">{{ summary.activeUsers }}</p>
      </div>
      <div class="bg-red-50 p-4 rounded-lg border-l-4 border-red-500">
        <p class="text-gray-600 text-sm">Bị Khóa</p>
        <p class="text-2xl font-bold text-red-600">{{ summary.lockedUsers }}</p>
      </div>
      <div class="bg-orange-50 p-4 rounded-lg border-l-4 border-orange-500">
        <p class="text-gray-600 text-sm">Phải Đổi Mật Khẩu</p>
        <p class="text-2xl font-bold text-orange-600">{{ summary.mustChangePassword }}</p>
      </div>
    </div>

    <!-- Filters -->
    <div class="bg-white p-4 rounded-lg shadow mb-6">
      <div class="flex gap-4 flex-wrap">
        <div class="flex-1 min-w-48">
          <label class="block text-sm font-medium text-gray-700 mb-1">Tìm Kiếm</label>
          <input v-model="filters.search" type="text" placeholder="Username, email..." class="w-full px-3 py-2 border border-gray-300 rounded-md" />
        </div>
        <div class="flex-1 min-w-48">
          <label class="block text-sm font-medium text-gray-700 mb-1">Chức Vụ</label>
          <select v-model="filters.role" class="w-full px-3 py-2 border border-gray-300 rounded-md">
            <option value="">-- Tất Cả --</option>
            <option value="Admin">Admin</option>
            <option value="Teacher">Giáo Viên</option>
            <option value="Student">Học Viên</option>
          </select>
        </div>
        <div class="flex-1 min-w-48">
          <label class="block text-sm font-medium text-gray-700 mb-1">Trạng Thái</label>
          <select v-model="filters.status" class="w-full px-3 py-2 border border-gray-300 rounded-md">
            <option value="">-- Tất Cả --</option>
            <option value="Active">Hoạt Động</option>
            <option value="Locked">Bị Khóa</option>
          </select>
        </div>
        <div class="flex items-end">
          <button @click="loadUsers" class="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700">
            Tìm Kiếm
          </button>
        </div>
      </div>
    </div>

    <!-- Users Table -->
    <div class="bg-white p-4 rounded-lg shadow overflow-x-auto">
      <table class="min-w-full border-collapse border border-gray-300">
        <thead class="bg-gray-100">
          <tr>
            <th class="border border-gray-300 px-4 py-2 text-left">Username</th>
            <th class="border border-gray-300 px-4 py-2 text-left">Email</th>
            <th class="border border-gray-300 px-4 py-2 text-left">Họ Tên</th>
            <th class="border border-gray-300 px-4 py-2 text-left">Chức Vụ</th>
            <th class="border border-gray-300 px-4 py-2 text-center">Trạng Thái</th>
            <th class="border border-gray-300 px-4 py-2 text-center">Hành Động</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="user in users" :key="user.id" class="hover:bg-gray-50">
            <td class="border border-gray-300 px-4 py-2 font-semibold">{{ user.username }}</td>
            <td class="border border-gray-300 px-4 py-2">{{ user.email }}</td>
            <td class="border border-gray-300 px-4 py-2">{{ user.fullName }}</td>
            <td class="border border-gray-300 px-4 py-2">{{ getRoleName(user.role) }}</td>
            <td class="border border-gray-300 px-4 py-2 text-center">
              <StatusBadge :status="user.status" type="user" />
            </td>
            <td class="border border-gray-300 px-4 py-2 text-center space-x-2">
              <button @click="selectUser(user)" class="text-blue-600 hover:underline">Chi Tiết</button>
              <button v-if="user.status === 'Active'" @click="toggleLockUser(user.id, 'Locked')" class="text-red-600 hover:underline">Khóa</button>
              <button v-else @click="toggleLockUser(user.id, 'Active')" class="text-green-600 hover:underline">Mở Khóa</button>
              <button @click="resetUserPassword(user.id)" class="text-orange-600 hover:underline">Reset Mật Khẩu</button>
            </td>
          </tr>
        </tbody>
      </table>
      <div v-if="users.length === 0" class="text-center py-8 text-gray-500">
        Không tìm thấy tài khoản nào
      </div>
    </div>

    <!-- Create User Modal -->
    <div v-if="showCreateForm" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center">
      <div class="bg-white p-6 rounded-lg max-w-lg w-full">
        <h2 class="text-xl font-bold mb-4">Tạo Tài Khoản Mới</h2>
        <UserForm @submit="createUser" @cancel="showCreateForm = false" />
      </div>
    </div>

    <!-- User Detail Modal -->
    <div v-if="selectedUser" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center">
      <div class="bg-white p-6 rounded-lg max-w-lg w-full">
        <h2 class="text-xl font-bold mb-4">Chi Tiết Tài Khoản</h2>
        <div class="space-y-3 mb-4">
          <p><strong>Username:</strong> {{ selectedUser.username }}</p>
          <p><strong>Email:</strong> {{ selectedUser.email }}</p>
          <p><strong>Họ Tên:</strong> {{ selectedUser.fullName }}</p>
          <p><strong>Chức Vụ:</strong> {{ getRoleName(selectedUser.role) }}</p>
          <p><strong>Trạng Thái:</strong> <StatusBadge :status="selectedUser.status" type="user" /></p>
          <p v-if="selectedUser.mustChangePassword" class="text-orange-600"><strong>⚠️ Phải đổi mật khẩu lần đầu</strong></p>
        </div>
        <button @click="selectedUser = null" class="w-full bg-gray-400 text-white py-2 rounded-md hover:bg-gray-500">
          Đóng
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import api from '../api';
import UserForm from '../components/UserForm.vue';
import StatusBadge from '../components/StatusBadge.vue';

const users = ref([]);
const selectedUser = ref(null);
const showCreateForm = ref(false);

const filters = ref({
  search: '',
  role: '',
  status: ''
});

const summary = ref({
  totalUsers: 0,
  activeUsers: 0,
  lockedUsers: 0,
  mustChangePassword: 0
});

const loadUsers = async () => {
  try {
    const response = await api.get('/admin/users');
    let data = response.data;

    if (filters.value.search) {
      data = data.filter((u: any) => 
        u.username.toLowerCase().includes(filters.value.search.toLowerCase()) ||
        u.email.toLowerCase().includes(filters.value.search.toLowerCase())
      );
    }

    if (filters.value.role) {
      data = data.filter((u: any) => u.role === filters.value.role);
    }

    if (filters.value.status) {
      data = data.filter((u: any) => u.status === filters.value.status);
    }

    users.value = data;
    calculateSummary(data);
  } catch (error) {
    console.error('Error loading users:', error);
  }
};

const calculateSummary = (data: any[]) => {
  summary.value = {
    totalUsers: data.length,
    activeUsers: data.filter(u => u.status === 'Active').length,
    lockedUsers: data.filter(u => u.status === 'Locked').length,
    mustChangePassword: data.filter(u => u.mustChangePassword).length
  };
};

const createUser = async (formData: any) => {
  try {
    await api.post('/admin/users', formData);
    showCreateForm.value = false;
    loadUsers();
    alert('Tạo tài khoản thành công');
  } catch (error) {
    console.error('Error creating user:', error);
  }
};

const selectUser = (user: any) => {
  selectedUser.value = user;
};

const toggleLockUser = async (userId: number, newStatus: string) => {
  try {
    await api.patch(`/admin/users/${userId}/status`, { status: newStatus });
    loadUsers();
  } catch (error) {
    console.error('Error updating user status:', error);
  }
};

const resetUserPassword = async (userId: number) => {
  if (confirm('Bạn có chắc muốn reset mật khẩu cho tài khoản này?')) {
    try {
      await api.post(`/admin/users/${userId}/reset-password`);
      alert('Reset mật khẩu thành công. Mật khẩu tạm: Temp@123');
    } catch (error) {
      console.error('Error resetting password:', error);
    }
  }
};

const getRoleName = (role: string) => {
  const roles: { [key: string]: string } = {
    'Admin': 'Quản Trị Viên',
    'Teacher': 'Giáo Viên',
    'Student': 'Học Viên'
  };
  return roles[role] || role;
};

onMounted(() => {
  loadUsers();
});
</script>
