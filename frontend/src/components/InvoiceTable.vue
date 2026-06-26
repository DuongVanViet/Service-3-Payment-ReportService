<template>
  <div class="overflow-x-auto">
    <table class="min-w-full border-collapse border border-gray-300">
      <thead class="bg-gray-100">
        <tr>
          <th class="border border-gray-300 px-4 py-2 text-left">Mã HĐ</th>
          <th class="border border-gray-300 px-4 py-2 text-left">Học Viên</th>
          <th class="border border-gray-300 px-4 py-2 text-right">Tổng Tiền</th>
          <th class="border border-gray-300 px-4 py-2 text-right">Đã Thanh Toán</th>
          <th class="border border-gray-300 px-4 py-2 text-right">Còn Nợ</th>
          <th class="border border-gray-300 px-4 py-2 text-center">Trạng Thái</th>
          <th class="border border-gray-300 px-4 py-2 text-left">Hạn Thanh Toán</th>
          <th class="border border-gray-300 px-4 py-2 text-center">Hành Động</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="invoice in invoices" :key="invoice.id" class="hover:bg-gray-50">
          <td class="border border-gray-300 px-4 py-2 font-semibold">{{ invoice.invoiceCode }}</td>
          <td class="border border-gray-300 px-4 py-2">{{ invoice.studentName }}</td>
          <td class="border border-gray-300 px-4 py-2 text-right">{{ formatCurrency(invoice.finalAmount) }}</td>
          <td class="border border-gray-300 px-4 py-2 text-right font-semibold text-green-600">{{ formatCurrency(invoice.paidAmount) }}</td>
          <td class="border border-gray-300 px-4 py-2 text-right font-semibold" :class="invoice.debtAmount > 0 ? 'text-red-600' : 'text-green-600'">{{ formatCurrency(invoice.debtAmount) }}</td>
          <td class="border border-gray-300 px-4 py-2 text-center">
            <StatusBadge :status="invoice.status" type="invoice" />
          </td>
          <td class="border border-gray-300 px-4 py-2">{{ formatDate(invoice.dueDate) }}</td>
          <td class="border border-gray-300 px-4 py-2 text-center space-x-2">
            <button @click="$emit('view', invoice.id)" class="text-blue-600 hover:underline">Xem</button>
            <button v-if="showPaymentBtn && invoice.debtAmount > 0" @click="$emit('payment', invoice)" class="text-green-600 hover:underline">Thanh Toán</button>
            <button v-if="showCancelBtn" @click="$emit('cancel', invoice.id)" class="text-red-600 hover:underline">Hủy</button>
          </td>
        </tr>
      </tbody>
    </table>
    <div v-if="invoices.length === 0" class="text-center py-8 text-gray-500">
      Không có hóa đơn nào
    </div>
  </div>
</template>

<script setup lang="ts">
import StatusBadge from './StatusBadge.vue';

interface Props {
  invoices: any[];
  showPaymentBtn?: boolean;
  showCancelBtn?: boolean;
}

withDefaults(defineProps<Props>(), {
  showPaymentBtn: false,
  showCancelBtn: false
});

defineEmits(['view', 'payment', 'cancel']);

const formatCurrency = (value: number) => {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
};

const formatDate = (date: string) => {
  return new Date(date).toLocaleDateString('vi-VN');
};
</script>
