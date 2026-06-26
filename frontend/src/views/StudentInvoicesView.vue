<template>
  <div class="container mx-auto p-6">
    <div class="mb-6">
      <h1 class="text-3xl font-bold mb-2">Học Phí Của Tôi</h1>
      <p class="text-gray-600">Xem danh sách hóa đơn học phí cá nhân</p>
    </div>

    <!-- Summary Cards -->
    <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-8">
      <div class="bg-blue-50 p-4 rounded-lg border-l-4 border-blue-500">
        <p class="text-gray-600 text-sm">Tổng Số Hóa Đơn</p>
        <p class="text-2xl font-bold text-blue-600">{{ summary.invoiceCount }}</p>
      </div>
      <div class="bg-green-50 p-4 rounded-lg border-l-4 border-green-500">
        <p class="text-gray-600 text-sm">Tổng Đã Thanh Toán</p>
        <p class="text-2xl font-bold text-green-600">{{ formatCurrency(summary.totalPaid) }}</p>
      </div>
      <div class="bg-red-50 p-4 rounded-lg border-l-4 border-red-500">
        <p class="text-gray-600 text-sm">Còn Nợ</p>
        <p class="text-2xl font-bold text-red-600">{{ formatCurrency(summary.totalDebt) }}</p>
      </div>
      <div class="bg-orange-50 p-4 rounded-lg border-l-4 border-orange-500">
        <p class="text-gray-600 text-sm">Quá Hạn</p>
        <p class="text-2xl font-bold text-orange-600">{{ summary.overdueCount }}</p>
      </div>
    </div>

    <!-- Filters -->
    <div class="bg-white p-4 rounded-lg shadow mb-6 flex gap-4">
      <select v-model="filters.status" class="px-3 py-2 border border-gray-300 rounded-md">
        <option value="">-- Tất Cả Trạng Thái --</option>
        <option value="Paid">Đã Thanh Toán</option>
        <option value="Unpaid">Chưa Thanh Toán</option>
        <option value="Partial">Thanh Toán Một Phần</option>
        <option value="Overdue">Quá Hạn</option>
      </select>
      <button @click="loadInvoices" class="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700">
        Tải Lại
      </button>
    </div>

    <!-- Invoice List -->
    <div class="bg-white p-4 rounded-lg shadow">
      <InvoiceTable :invoices="invoices" />
    </div>

    <!-- Invoice Detail Modal -->
    <div v-if="selectedInvoice" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center">
      <div class="bg-white p-6 rounded-lg max-w-lg w-full">
        <h2 class="text-xl font-bold mb-4">Chi Tiết Hóa Đơn</h2>
        <div class="space-y-2 mb-4">
          <p><strong>Mã HĐ:</strong> {{ selectedInvoice.invoiceCode }}</p>
          <p><strong>Khóa Học:</strong> {{ selectedInvoice.courseId }}</p>
          <p><strong>Lớp:</strong> {{ selectedInvoice.classId }}</p>
          <p><strong>Tổng Tiền:</strong> {{ formatCurrency(selectedInvoice.finalAmount) }}</p>
          <p><strong>Đã Thanh Toán:</strong> {{ formatCurrency(selectedInvoice.paidAmount) }}</p>
          <p><strong>Còn Nợ:</strong> {{ formatCurrency(selectedInvoice.debtAmount) }}</p>
          <p><strong>Hạn Thanh Toán:</strong> {{ formatDate(selectedInvoice.dueDate) }}</p>
          <p><strong>Trạng Thái:</strong> <StatusBadge :status="selectedInvoice.status" type="invoice" /></p>
        </div>
        <button @click="selectedInvoice = null" class="w-full bg-gray-400 text-white py-2 rounded-md hover:bg-gray-500">
          Đóng
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import api from '../api';
import InvoiceTable from '../components/InvoiceTable.vue';
import StatusBadge from '../components/StatusBadge.vue';

const invoices = ref([]);
const selectedInvoice = ref(null);
const filters = ref({ status: '' });

const summary = ref({
  invoiceCount: 0,
  totalDebt: 0,
  totalPaid: 0,
  overdueCount: 0
});

const loadInvoices = async () => {
  try {
    const response = await api.get('/student/invoices');
    let data = response.data;

    if (filters.value.status) {
      data = data.filter((inv: any) => inv.status === filters.value.status);
    }

    invoices.value = data;
    calculateSummary(data);
  } catch (error) {
    console.error('Error loading invoices:', error);
  }
};

const calculateSummary = (data: any[]) => {
  summary.value = {
    invoiceCount: data.length,
    totalDebt: data.reduce((sum, inv) => sum + inv.debtAmount, 0),
    totalPaid: data.reduce((sum, inv) => sum + inv.paidAmount, 0),
    overdueCount: data.filter(inv => inv.status === 'Overdue').length
  };
};

const formatCurrency = (value: number) => {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
};

const formatDate = (date: string) => {
  return new Date(date).toLocaleDateString('vi-VN');
};

onMounted(() => {
  loadInvoices();
});
</script>
