<template>
  <div class="container mx-auto p-6">
    <div class="mb-6">
      <h1 class="text-3xl font-bold mb-2">Lịch Sử Thanh Toán</h1>
      <p class="text-gray-600">Xem các giao dịch thanh toán của bạn</p>
    </div>

    <!-- Summary -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
      <div class="bg-green-50 p-4 rounded-lg border-l-4 border-green-500">
        <p class="text-gray-600 text-sm">Tổng Đã Thanh Toán</p>
        <p class="text-2xl font-bold text-green-600">{{ formatCurrency(summary.totalPaid) }}</p>
      </div>
      <div class="bg-blue-50 p-4 rounded-lg border-l-4 border-blue-500">
        <p class="text-gray-600 text-sm">Số Giao Dịch</p>
        <p class="text-2xl font-bold text-blue-600">{{ summary.transactionCount }}</p>
      </div>
      <div class="bg-purple-50 p-4 rounded-lg border-l-4 border-purple-500">
        <p class="text-gray-600 text-sm">Giao Dịch Gần Đây</p>
        <p class="text-2xl font-bold text-purple-600">{{ summary.recentCount }}</p>
      </div>
    </div>

    <!-- Filters -->
    <div class="bg-white p-4 rounded-lg shadow mb-6">
      <div class="flex gap-4 flex-wrap">
        <div class="flex-1 min-w-48">
          <label class="block text-sm font-medium text-gray-700 mb-1">Từ Ngày</label>
          <input v-model="filters.fromDate" type="date" class="w-full px-3 py-2 border border-gray-300 rounded-md" />
        </div>
        <div class="flex-1 min-w-48">
          <label class="block text-sm font-medium text-gray-700 mb-1">Đến Ngày</label>
          <input v-model="filters.toDate" type="date" class="w-full px-3 py-2 border border-gray-300 rounded-md" />
        </div>
        <div class="flex-1 min-w-48">
          <label class="block text-sm font-medium text-gray-700 mb-1">Phương Thức</label>
          <select v-model="filters.method" class="w-full px-3 py-2 border border-gray-300 rounded-md">
            <option value="">-- Tất Cả --</option>
            <option value="Cash">Tiền Mặt</option>
            <option value="BankTransfer">Chuyển Khoản</option>
            <option value="Online">Thanh Toán Online</option>
          </select>
        </div>
        <div class="flex items-end">
          <button @click="loadPayments" class="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700">
            Tìm Kiếm
          </button>
        </div>
      </div>
    </div>

    <!-- Payments Table -->
    <div class="bg-white p-4 rounded-lg shadow overflow-x-auto">
      <table class="min-w-full border-collapse border border-gray-300">
        <thead class="bg-gray-100">
          <tr>
            <th class="border border-gray-300 px-4 py-2 text-left">Mã HĐ</th>
            <th class="border border-gray-300 px-4 py-2 text-right">Số Tiền</th>
            <th class="border border-gray-300 px-4 py-2 text-left">Phương Thức</th>
            <th class="border border-gray-300 px-4 py-2 text-left">Người Ghi Nhận</th>
            <th class="border border-gray-300 px-4 py-2 text-left">Ngày Thanh Toán</th>
            <th class="border border-gray-300 px-4 py-2 text-left">Ghi Chú</th>
            <th class="border border-gray-300 px-4 py-2 text-center">Hành Động</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="payment in payments" :key="payment.id" class="hover:bg-gray-50">
            <td class="border border-gray-300 px-4 py-2 font-semibold">{{ payment.invoiceCode }}</td>
            <td class="border border-gray-300 px-4 py-2 text-right font-semibold text-green-600">{{ formatCurrency(payment.amount) }}</td>
            <td class="border border-gray-300 px-4 py-2">{{ getPaymentMethodName(payment.paymentMethod) }}</td>
            <td class="border border-gray-300 px-4 py-2">{{ payment.collectedBy }}</td>
            <td class="border border-gray-300 px-4 py-2">{{ formatDate(payment.paidAt) }}</td>
            <td class="border border-gray-300 px-4 py-2 text-sm">{{ payment.note || '-' }}</td>
            <td class="border border-gray-300 px-4 py-2 text-center">
              <button @click="viewReceipt(payment.id)" class="text-blue-600 hover:underline">Biên Lai</button>
            </td>
          </tr>
        </tbody>
      </table>
      <div v-if="payments.length === 0" class="text-center py-8 text-gray-500">
        Không có giao dịch nào
      </div>
    </div>

    <!-- Receipt Modal -->
    <div v-if="selectedReceipt" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center">
      <div class="bg-white p-8 rounded-lg max-w-lg w-full">
        <h2 class="text-2xl font-bold mb-4">Biên Lai Thanh Toán</h2>
        <div class="border-t-2 border-b-2 py-4 mb-4">
          <div class="text-center mb-4">
            <p class="font-bold text-lg">BIÊN LAI THANH TOÁN</p>
            <p class="text-gray-600">{{ formatDate(new Date().toISOString()) }}</p>
          </div>
          <div class="space-y-2 text-sm">
            <p><strong>Mã Biên Lai:</strong> {{ selectedReceipt.id }}</p>
            <p><strong>Mã HĐ:</strong> {{ selectedReceipt.invoiceCode }}</p>
            <p><strong>Số Tiền:</strong> {{ formatCurrency(selectedReceipt.amount) }}</p>
            <p><strong>Phương Thức:</strong> {{ getPaymentMethodName(selectedReceipt.paymentMethod) }}</p>
            <p><strong>Người Ghi Nhận:</strong> {{ selectedReceipt.collectedBy }}</p>
            <p><strong>Ngày:</strong> {{ formatDate(selectedReceipt.paidAt) }}</p>
            <p v-if="selectedReceipt.note"><strong>Ghi Chú:</strong> {{ selectedReceipt.note }}</p>
          </div>
        </div>
        <div class="flex gap-2">
          <button @click="printReceipt" class="flex-1 bg-blue-600 text-white py-2 rounded-md hover:bg-blue-700">
            In Biên Lai
          </button>
          <button @click="selectedReceipt = null" class="flex-1 bg-gray-400 text-white py-2 rounded-md hover:bg-gray-500">
            Đóng
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import api from '../api';

const payments = ref([]);
const selectedReceipt = ref(null);
const filters = ref({ fromDate: '', toDate: '', method: '' });

const summary = ref({
  totalPaid: 0,
  transactionCount: 0,
  recentCount: 0
});

const loadPayments = async () => {
  try {
    const response = await api.get('/student/payments');
    let data = response.data;

    if (filters.value.method) {
      data = data.filter((p: any) => p.paymentMethod === filters.value.method);
    }

    payments.value = data;
    calculateSummary(data);
  } catch (error) {
    console.error('Error loading payments:', error);
  }
};

const calculateSummary = (data: any[]) => {
  const now = new Date();
  const thirtyDaysAgo = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000);

  summary.value = {
    totalPaid: data.reduce((sum, p) => sum + p.amount, 0),
    transactionCount: data.length,
    recentCount: data.filter(p => new Date(p.paidAt) > thirtyDaysAgo).length
  };
};

const viewReceipt = (paymentId: number) => {
  selectedReceipt.value = payments.value.find(p => p.id === paymentId);
};

const printReceipt = () => {
  window.print();
};

const formatCurrency = (value: number) => {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
};

const formatDate = (date: string) => {
  return new Date(date).toLocaleDateString('vi-VN', { 
    year: 'numeric', 
    month: '2-digit', 
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  });
};

const getPaymentMethodName = (method: string) => {
  const methods: { [key: string]: string } = {
    'Cash': 'Tiền Mặt',
    'BankTransfer': 'Chuyển Khoản',
    'Online': 'Thanh Toán Online'
  };
  return methods[method] || method;
};

onMounted(() => {
  loadPayments();
});
</script>
