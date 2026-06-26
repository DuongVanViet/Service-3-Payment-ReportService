<template>
  <div class="container mx-auto p-6">
    <div class="mb-6 flex justify-between items-center">
      <div>
        <h1 class="text-3xl font-bold mb-2">Quản Lý Hóa Đơn</h1>
        <p class="text-gray-600">Quản lý và theo dõi tất cả hóa đơn học phí</p>
      </div>
      <button @click="showCreateForm = true" class="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700">
        + Tạo Hóa Đơn Mới
      </button>
    </div>

    <!-- Summary -->
    <div class="grid grid-cols-1 md:grid-cols-5 gap-4 mb-8">
      <div class="bg-blue-50 p-4 rounded-lg border-l-4 border-blue-500">
        <p class="text-gray-600 text-sm">Tổng Hóa Đơn</p>
        <p class="text-2xl font-bold text-blue-600">{{ summary.totalInvoices }}</p>
      </div>
      <div class="bg-green-50 p-4 rounded-lg border-l-4 border-green-500">
        <p class="text-gray-600 text-sm">Tổng Thu</p>
        <p class="text-2xl font-bold text-green-600">{{ formatCurrency(summary.totalRevenue) }}</p>
      </div>
      <div class="bg-red-50 p-4 rounded-lg border-l-4 border-red-500">
        <p class="text-gray-600 text-sm">Tổng Nợ</p>
        <p class="text-2xl font-bold text-red-600">{{ formatCurrency(summary.totalDebt) }}</p>
      </div>
      <div class="bg-orange-50 p-4 rounded-lg border-l-4 border-orange-500">
        <p class="text-gray-600 text-sm">Quá Hạn</p>
        <p class="text-2xl font-bold text-orange-600">{{ summary.overdueCount }}</p>
      </div>
      <div class="bg-yellow-50 p-4 rounded-lg border-l-4 border-yellow-500">
        <p class="text-gray-600 text-sm">% Thu</p>
        <p class="text-2xl font-bold text-yellow-600">{{ summary.collectionRate }}%</p>
      </div>
    </div>

    <!-- Filters -->
    <div class="bg-white p-4 rounded-lg shadow mb-6">
      <div class="flex gap-4 flex-wrap">
        <div class="flex-1 min-w-48">
          <label class="block text-sm font-medium text-gray-700 mb-1">Trạng Thái</label>
          <select v-model="filters.status" class="w-full px-3 py-2 border border-gray-300 rounded-md">
            <option value="">-- Tất Cả --</option>
            <option value="Paid">Đã Thanh Toán</option>
            <option value="Partial">Một Phần</option>
            <option value="Unpaid">Chưa Thanh Toán</option>
            <option value="Overdue">Quá Hạn</option>
          </select>
        </div>
        <div class="flex items-end">
          <button @click="loadInvoices" class="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700">
            Tải Lại
          </button>
          <button @click="exportData" class="ml-2 bg-green-600 text-white px-4 py-2 rounded-md hover:bg-green-700">
            Xuất Excel
          </button>
        </div>
      </div>
    </div>

    <!-- Invoices Table -->
    <div class="bg-white p-4 rounded-lg shadow">
      <InvoiceTable :invoices="invoices" :showPaymentBtn="true" :showCancelBtn="true" @payment="showPaymentForm" @cancel="cancelInvoice" />
    </div>

    <!-- Create Invoice Modal -->
    <div v-if="showCreateForm" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center">
      <div class="bg-white p-6 rounded-lg max-w-lg w-full">
        <h2 class="text-xl font-bold mb-4">Tạo Hóa Đơn Mới</h2>
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700">Học Viên ID</label>
            <input v-model.number="newInvoice.studentId" type="number" class="w-full px-3 py-2 border border-gray-300 rounded-md" />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700">Enrollment ID</label>
            <input v-model.number="newInvoice.enrollmentId" type="number" class="w-full px-3 py-2 border border-gray-300 rounded-md" />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700">Khóa Học ID</label>
            <input v-model.number="newInvoice.courseId" type="number" class="w-full px-3 py-2 border border-gray-300 rounded-md" />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700">Lớp ID</label>
            <input v-model.number="newInvoice.classId" type="number" class="w-full px-3 py-2 border border-gray-300 rounded-md" />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700">Tổng Tiền</label>
            <input v-model.number="newInvoice.totalAmount" type="number" step="0.01" class="w-full px-3 py-2 border border-gray-300 rounded-md" />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700">Giảm Giá</label>
            <input v-model.number="newInvoice.discountAmount" type="number" step="0.01" class="w-full px-3 py-2 border border-gray-300 rounded-md" />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700">Hạn Thanh Toán</label>
            <input v-model="newInvoice.dueDate" type="date" class="w-full px-3 py-2 border border-gray-300 rounded-md" />
          </div>
          <div class="flex gap-2">
            <button @click="createInvoice" class="flex-1 bg-blue-600 text-white py-2 rounded-md hover:bg-blue-700">Tạo</button>
            <button @click="showCreateForm = false" class="flex-1 bg-gray-400 text-white py-2 rounded-md hover:bg-gray-500">Hủy</button>
          </div>
        </div>
      </div>
    </div>

    <!-- Payment Modal -->
    <div v-if="showPayment && selectedForPayment" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center">
      <div class="bg-white p-6 rounded-lg max-w-lg w-full">
        <PaymentForm :invoiceInfo="selectedForPayment" @submit="recordPayment" @cancel="showPayment = false" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import api from '../api';
import InvoiceTable from '../components/InvoiceTable.vue';
import PaymentForm from '../components/PaymentForm.vue';

const invoices = ref([]);
const selectedForPayment = ref(null);
const showCreateForm = ref(false);
const showPayment = ref(false);

const filters = ref({ status: '' });

const summary = ref({
  totalInvoices: 0,
  totalRevenue: 0,
  totalDebt: 0,
  overdueCount: 0,
  collectionRate: 0
});

const newInvoice = ref({
  studentId: 0,
  enrollmentId: 0,
  courseId: 0,
  classId: 0,
  totalAmount: 0,
  discountAmount: 0,
  dueDate: ''
});

const loadInvoices = async () => {
  try {
    const response = await api.get('/admin/invoices');
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
  const totalRevenue = data.filter(i => i.status === 'Paid').reduce((sum, i) => sum + i.finalAmount, 0);
  const totalDebt = data.reduce((sum, i) => sum + i.debtAmount, 0);
  const totalAmount = data.reduce((sum, i) => sum + i.finalAmount, 0);

  summary.value = {
    totalInvoices: data.length,
    totalRevenue,
    totalDebt,
    overdueCount: data.filter(i => i.status === 'Overdue').length,
    collectionRate: totalAmount > 0 ? Math.round((totalRevenue / totalAmount) * 100) : 0
  };
};

const createInvoice = async () => {
  try {
    const response = await api.post('/admin/invoices', newInvoice.value);
    if (response.status === 201) {
      showCreateForm.value = false;
      newInvoice.value = { studentId: 0, enrollmentId: 0, courseId: 0, classId: 0, totalAmount: 0, discountAmount: 0, dueDate: '' };
      loadInvoices();
      alert('Tạo hóa đơn thành công');
    }
  } catch (error) {
    console.error('Error creating invoice:', error);
    alert('Lỗi khi tạo hóa đơn');
  }
};

const showPaymentForm = (invoice: any) => {
  selectedForPayment.value = invoice;
  showPayment.value = true;
};

const recordPayment = async (paymentData: any) => {
  try {
    const response = await api.post(`/admin/invoices/${selectedForPayment.value.id}/payments`, paymentData);
    if (response.status === 201) {
      showPayment.value = false;
      loadInvoices();
      alert('Ghi nhận thanh toán thành công');
    }
  } catch (error) {
    console.error('Error recording payment:', error);
    alert('Lỗi khi ghi nhận thanh toán');
  }
};

const cancelInvoice = async (invoiceId: number) => {
  if (confirm('Bạn có chắc muốn hủy hóa đơn này?')) {
    try {
      const response = await api.post(`/admin/invoices/${invoiceId}/cancel`);
      if (response.status === 204) {
        loadInvoices();
        alert('Hủy hóa đơn thành công');
      }
    } catch (error) {
      console.error('Error cancelling invoice:', error);
      alert('Lỗi khi hủy hóa đơn');
    }
  }
};

const exportData = () => {
  // CSV export logic
  const csv = [['Mã HĐ', 'Học Viên', 'Tổng Tiền', 'Đã Thanh Toán', 'Còn Nợ', 'Trạng Thái', 'Hạn']];
  invoices.value.forEach(inv => {
    csv.push([inv.invoiceCode, inv.studentId, inv.finalAmount, inv.paidAmount, inv.debtAmount, inv.status, inv.dueDate]);
  });
  
  const csvContent = csv.map(row => row.join(',')).join('\n');
  const blob = new Blob([csvContent], { type: 'text/csv' });
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = 'invoices.csv';
  a.click();
};

const formatCurrency = (value: number) => {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
};

onMounted(() => {
  loadInvoices();
});
</script>
