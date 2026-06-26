<template>
  <div class="container mx-auto p-6">
    <div class="mb-6">
      <h1 class="text-3xl font-bold mb-2">Quản Lý Công Nợ</h1>
      <p class="text-gray-600">Theo dõi và quản lý các khoản nợ của học viên</p>
    </div>

    <!-- Summary -->
    <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-8">
      <div class="bg-red-50 p-4 rounded-lg border-l-4 border-red-500">
        <p class="text-gray-600 text-sm">Tổng Công Nợ</p>
        <p class="text-2xl font-bold text-red-600">{{ formatCurrency(summary.totalDebt) }}</p>
      </div>
      <div class="bg-orange-50 p-4 rounded-lg border-l-4 border-orange-500">
        <p class="text-gray-600 text-sm">Nợ Quá Hạn</p>
        <p class="text-2xl font-bold text-orange-600">{{ formatCurrency(summary.overdueDebt) }}</p>
      </div>
      <div class="bg-blue-50 p-4 rounded-lg border-l-4 border-blue-500">
        <p class="text-gray-600 text-sm">Học Viên Nợ Tiền</p>
        <p class="text-2xl font-bold text-blue-600">{{ summary.studentInDebt }}</p>
      </div>
      <div class="bg-purple-50 p-4 rounded-lg border-l-4 border-purple-500">
        <p class="text-gray-600 text-sm">Hóa Đơn Quá Hạn</p>
        <p class="text-2xl font-bold text-purple-600">{{ summary.overdueInvoiceCount }}</p>
      </div>
    </div>

    <!-- Overdue Alert -->
    <div v-if="summary.overdueInvoiceCount > 0" class="bg-red-50 border border-red-300 p-4 rounded-lg mb-6">
      <p class="text-red-800 font-semibold">🚨 Có {{ summary.overdueInvoiceCount }} hóa đơn quá hạn cần xử lý</p>
    </div>

    <!-- Filters -->
    <div class="bg-white p-4 rounded-lg shadow mb-6">
      <div class="flex gap-4 flex-wrap">
        <div class="flex-1 min-w-48">
          <label class="block text-sm font-medium text-gray-700 mb-1">Sắp Xếp Theo</label>
          <select v-model="filters.sortBy" class="w-full px-3 py-2 border border-gray-300 rounded-md">
            <option value="debt_desc">Nợ Nhiều Nhất</option>
            <option value="debt_asc">Nợ Ít Nhất</option>
            <option value="overdue">Quá Hạn Trước</option>
          </select>
        </div>
        <div class="flex items-end">
          <button @click="loadDebts" class="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700">
            Tải Lại
          </button>
        </div>
      </div>
    </div>

    <!-- Debts Table -->
    <div class="space-y-4">
      <div v-for="studentDebt in studentDebts" :key="studentDebt.studentId" class="bg-white p-4 rounded-lg shadow">
        <div class="flex justify-between items-center mb-3 pb-3 border-b">
          <div>
            <h3 class="font-bold text-lg">Học Viên ID: {{ studentDebt.studentId }}</h3>
            <p class="text-gray-600 text-sm">{{ studentDebt.invoiceCount }} hóa đơn</p>
          </div>
          <div class="text-right">
            <p class="text-red-600 font-bold text-lg">{{ formatCurrency(studentDebt.totalDebt) }}</p>
            <p class="text-sm text-gray-600">Còn Nợ</p>
          </div>
        </div>

        <!-- Student Invoices -->
        <div class="space-y-2">
          <div v-for="invoice in studentDebt.invoices" :key="invoice.id" class="bg-gray-50 p-3 rounded flex justify-between items-center hover:bg-gray-100">
            <div class="flex-1">
              <p class="font-semibold">{{ invoice.invoiceCode }}</p>
              <p class="text-sm text-gray-600">
                Hạn: {{ formatDate(invoice.dueDate) }} 
                <span v-if="invoice.status === 'Overdue'" class="ml-2 text-red-600 font-semibold">(Quá Hạn)</span>
              </p>
            </div>
            <div class="text-right min-w-40">
              <div class="flex justify-between gap-4 text-sm">
                <div>
                  <p class="text-gray-600">Phải Thu</p>
                  <p class="font-semibold">{{ formatCurrency(invoice.finalAmount) }}</p>
                </div>
                <div>
                  <p class="text-gray-600">Nợ</p>
                  <p class="font-bold text-red-600">{{ formatCurrency(invoice.debtAmount) }}</p>
                </div>
              </div>
            </div>
            <button @click="selectInvoiceForPayment(invoice)" class="ml-4 bg-green-600 text-white px-3 py-1 rounded hover:bg-green-700 text-sm">
              Thanh Toán
            </button>
          </div>
        </div>
      </div>

      <div v-if="studentDebts.length === 0" class="bg-white p-8 rounded-lg shadow text-center text-gray-500">
        Không có công nợ
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
import { ref, computed, onMounted } from 'vue';
import api from '../api';
import PaymentForm from '../components/PaymentForm.vue';

const invoices = ref([]);
const selectedForPayment = ref(null);
const showPayment = ref(false);

const filters = ref({ sortBy: 'debt_desc' });

const summary = ref({
  totalDebt: 0,
  overdueDebt: 0,
  studentInDebt: 0,
  overdueInvoiceCount: 0
});

const studentDebts = computed(() => {
  const grouped = invoices.value.reduce((acc: any, inv: any) => {
    const existing = acc.find((s: any) => s.studentId === inv.studentId);
    if (existing) {
      existing.invoices.push(inv);
      existing.totalDebt += inv.debtAmount;
    } else {
      acc.push({
        studentId: inv.studentId,
        totalDebt: inv.debtAmount,
        invoiceCount: 1,
        invoices: [inv]
      });
    }
    return acc;
  }, []);

  // Sort by filter
  if (filters.value.sortBy === 'debt_asc') {
    grouped.sort((a: any, b: any) => a.totalDebt - b.totalDebt);
  } else if (filters.value.sortBy === 'overdue') {
    grouped.sort((a: any, b: any) => {
      const aHasOverdue = a.invoices.some((i: any) => i.status === 'Overdue');
      const bHasOverdue = b.invoices.some((i: any) => i.status === 'Overdue');
      return (bHasOverdue ? 1 : 0) - (aHasOverdue ? 1 : 0);
    });
  } else {
    grouped.sort((a: any, b: any) => b.totalDebt - a.totalDebt);
  }

  return grouped;
});

const loadDebts = async () => {
  try {
    const response = await api.get('/admin/invoices');
    const data = response.data;
    invoices.value = data.filter((inv: any) => inv.debtAmount > 0);
    calculateSummary(data);
  } catch (error) {
    console.error('Error loading debts:', error);
  }
};

const calculateSummary = (data: any[]) => {
  const withDebt = data.filter(i => i.debtAmount > 0);
  const overdue = withDebt.filter(i => i.status === 'Overdue');

  summary.value = {
    totalDebt: withDebt.reduce((sum, i) => sum + i.debtAmount, 0),
    overdueDebt: overdue.reduce((sum, i) => sum + i.debtAmount, 0),
    studentInDebt: new Set(withDebt.map(i => i.studentId)).size,
    overdueInvoiceCount: overdue.length
  };
};

const selectInvoiceForPayment = (invoice: any) => {
  selectedForPayment.value = invoice;
  showPayment.value = true;
};

const recordPayment = async (paymentData: any) => {
  try {
    await api.post(`/admin/invoices/${selectedForPayment.value.id}/payments`, paymentData);
    showPayment.value = false;
    loadDebts();
    alert('Ghi nhận thanh toán thành công');
  } catch (error) {
    console.error('Error recording payment:', error);
  }
};

const formatCurrency = (value: number) => {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
};

const formatDate = (date: string) => {
  return new Date(date).toLocaleDateString('vi-VN');
};

onMounted(() => {
  loadDebts();
});
</script>
