<template>
  <div class="container mx-auto p-6">
    <div class="mb-6">
      <h1 class="text-3xl font-bold mb-2">Công Nợ Của Tôi</h1>
      <p class="text-gray-600">Xem chi tiết các khoản nợ còn lại</p>
    </div>

    <!-- Debt Summary -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
      <div class="bg-red-50 p-4 rounded-lg border-l-4 border-red-500">
        <p class="text-gray-600 text-sm">Tổng Công Nợ</p>
        <p class="text-2xl font-bold text-red-600">{{ formatCurrency(debtSummary.totalDebt) }}</p>
      </div>
      <div class="bg-orange-50 p-4 rounded-lg border-l-4 border-orange-500">
        <p class="text-gray-600 text-sm">Hóa Đơn Quá Hạn</p>
        <p class="text-2xl font-bold text-orange-600">{{ debtSummary.overdueCount }}</p>
      </div>
      <div v-if="debtSummary.hasOverdueDebt" class="bg-red-100 p-4 rounded-lg border-l-4 border-red-700">
        <p class="text-gray-700 text-sm font-semibold">⚠️ Bạn có hóa đơn quá hạn</p>
        <p class="text-sm text-red-700">Vui lòng thanh toán sớm</p>
      </div>
    </div>

    <!-- Overdue Invoices -->
    <div v-if="overdueInvoices.length > 0" class="bg-orange-50 border border-orange-300 p-4 rounded-lg mb-6">
      <h3 class="text-lg font-semibold mb-3 text-orange-800">🔴 Hóa Đơn Quá Hạn</h3>
      <div class="space-y-2">
        <div v-for="invoice in overdueInvoices" :key="invoice.id" class="bg-white p-3 rounded flex justify-between items-center">
          <div>
            <p class="font-semibold">{{ invoice.invoiceCode }}</p>
            <p class="text-sm text-gray-600">Hạn: {{ formatDate(invoice.dueDate) }}</p>
          </div>
          <p class="text-red-600 font-bold">{{ formatCurrency(invoice.debtAmount) }}</p>
        </div>
      </div>
    </div>

    <!-- Partial Paid Invoices -->
    <div v-if="partialInvoices.length > 0" class="bg-white p-4 rounded-lg shadow">
      <h3 class="text-lg font-semibold mb-4">Hóa Đơn Thanh Toán Một Phần</h3>
      <div class="space-y-4">
        <div v-for="invoice in partialInvoices" :key="invoice.id" class="border-l-4 border-blue-500 p-4 bg-blue-50">
          <div class="flex justify-between items-center mb-2">
            <p class="font-semibold">{{ invoice.invoiceCode }}</p>
            <StatusBadge :status="invoice.status" type="invoice" />
          </div>
          <div class="grid grid-cols-3 gap-4 text-sm">
            <div>
              <p class="text-gray-600">Tổng Tiền</p>
              <p class="font-semibold">{{ formatCurrency(invoice.finalAmount) }}</p>
            </div>
            <div>
              <p class="text-gray-600">Đã Thanh Toán</p>
              <p class="font-semibold text-green-600">{{ formatCurrency(invoice.paidAmount) }}</p>
            </div>
            <div>
              <p class="text-gray-600">Còn Nợ</p>
              <p class="font-semibold text-red-600">{{ formatCurrency(invoice.debtAmount) }}</p>
            </div>
          </div>
          <div class="mt-2 w-full bg-gray-300 rounded-full h-2">
            <div class="bg-green-600 h-2 rounded-full" :style="{ width: (invoice.paidAmount / invoice.finalAmount * 100) + '%' }"></div>
          </div>
        </div>
      </div>
    </div>

    <!-- All Invoices with Debt -->
    <div class="mt-8 bg-white p-4 rounded-lg shadow">
      <h3 class="text-lg font-semibold mb-4">Tất Cả Hóa Đơn Còn Nợ</h3>
      <InvoiceTable :invoices="invoices" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import api from '../api';
import InvoiceTable from '../components/InvoiceTable.vue';
import StatusBadge from '../components/StatusBadge.vue';

const invoices = ref([]);
const debtSummary = ref({ totalDebt: 0, overdueCount: 0, hasOverdueDebt: false });

const overdueInvoices = computed(() => 
  invoices.value.filter(inv => inv.status === 'Overdue' && inv.debtAmount > 0)
);

const partialInvoices = computed(() => 
  invoices.value.filter(inv => inv.status === 'Partial' && inv.debtAmount > 0)
);

const loadDebt = async () => {
  try {
    const response = await api.get('/student/debt');
    debtSummary.value = response.data;

    const invoicesResponse = await api.get('/student/invoices');
    invoices.value = invoicesResponse.data.filter((inv: any) => inv.debtAmount > 0);
  } catch (error) {
    console.error('Error loading debt:', error);
  }
};

const formatCurrency = (value: number) => {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
};

const formatDate = (date: string) => {
  return new Date(date).toLocaleDateString('vi-VN');
};

onMounted(() => {
  loadDebt();
});
</script>
