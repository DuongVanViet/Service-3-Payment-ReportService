<template>
  <div class="bg-white p-6 rounded-lg shadow-md">
    <h3 class="text-lg font-semibold mb-4">Ghi Nhận Thanh Toán</h3>
    
    <div v-if="invoiceInfo" class="bg-blue-50 p-4 rounded mb-4">
      <p><strong>Mã Hóa Đơn:</strong> {{ invoiceInfo.invoiceCode }}</p>
      <p><strong>Học Viên:</strong> {{ invoiceInfo.studentName }}</p>
      <p><strong>Số Tiền Phải Thu:</strong> {{ formatCurrency(invoiceInfo.finalAmount) }}</p>
      <p><strong>Đã Thanh Toán:</strong> {{ formatCurrency(invoiceInfo.paidAmount) }}</p>
      <p><strong>Còn Nợ:</strong> <span class="font-semibold text-red-600">{{ formatCurrency(invoiceInfo.debtAmount) }}</span></p>
    </div>

    <div class="space-y-4">
      <div>
        <label class="block text-sm font-medium text-gray-700">Số Tiền Thanh Toán</label>
        <input v-model.number="form.amount" type="number" step="0.01" class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500" />
      </div>

      <div>
        <label class="block text-sm font-medium text-gray-700">Phương Thức Thanh Toán</label>
        <select v-model="form.paymentMethod" class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500">
          <option value="">-- Chọn phương thức --</option>
          <option value="Cash">Tiền Mặt</option>
          <option value="BankTransfer">Chuyển Khoản</option>
          <option value="Online">Thanh Toán Online</option>
        </select>
      </div>

      <div>
        <label class="block text-sm font-medium text-gray-700">Người Ghi Nhận</label>
        <input v-model="form.collectedBy" type="text" class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500" placeholder="Tên admin" />
      </div>

      <div>
        <label class="block text-sm font-medium text-gray-700">Ghi Chú</label>
        <textarea v-model="form.note" class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500" rows="3" placeholder="Ghi chú thêm..."></textarea>
      </div>

      <div class="flex gap-4">
        <button @click="submit" class="flex-1 bg-green-600 text-white py-2 rounded-md hover:bg-green-700">
          Ghi Nhận Thanh Toán
        </button>
        <button @click="cancel" class="flex-1 bg-gray-400 text-white py-2 rounded-md hover:bg-gray-500">
          Hủy
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';

interface Props {
  invoiceInfo?: any;
}

interface Emits {
  (e: 'submit', data: any): void;
  (e: 'cancel'): void;
}

defineProps<Props>();
const emit = defineEmits<Emits>();

const form = ref({
  amount: 0,
  paymentMethod: '',
  collectedBy: '',
  note: ''
});

const submit = () => {
  emit('submit', form.value);
  form.value = { amount: 0, paymentMethod: '', collectedBy: '', note: '' };
};

const cancel = () => {
  emit('cancel');
};

const formatCurrency = (value: number) => {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
};
</script>
