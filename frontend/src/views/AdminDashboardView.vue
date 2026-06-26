<template>
  <div class="admin-dashboard">
    <header class="header">
      <div class="header-content">
        <h1>Admin Dashboard</h1>
        <div class="header-actions">
          <div class="user-info">
            <span>{{ authStore.user?.email }}</span>
            <button @click="handleLogout" class="btn-logout">Logout</button>
          </div>
        </div>
      </div>
    </header>

    <div class="container">
      <div class="tabs">
        <button
          @click="activeTab = 'invoices'"
          :class="['tab-btn', { active: activeTab === 'invoices' }]"
        >
          Hóa đơn
        </button>
        <button
          @click="activeTab = 'payments'"
          :class="['tab-btn', { active: activeTab === 'payments' }]"
        >
          Thanh toán
        </button>
      </div>

      <!-- Invoices Tab -->
      <section v-if="activeTab === 'invoices'" class="tab-content">
        <div class="section-header">
          <h2>Quản lý hóa đơn</h2>
          <button @click="loadInvoices" :disabled="isLoading" class="btn-primary">
            {{ isLoading ? 'Đang tải...' : 'Tải hóa đơn' }}
          </button>
        </div>

        <div v-if="invoiceError" class="error-message">{{ invoiceError }}</div>

        <div v-if="invoices.length > 0" class="invoices-list">
          <table>
            <thead>
              <tr>
                <th>Mã hóa đơn</th>
                <th>Học sinh</th>
                <th>Tổng tiền</th>
                <th>Đã trả</th>
                <th>Còn nợ</th>
                <th>Trạng thái</th>
                <th>Ngày đến hạn</th>
                <th>Hành động</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="invoice in invoices" :key="invoice.id">
                <td>{{ invoice.invoiceCode }}</td>
                <td>{{ invoice.studentName }}</td>
                <td>{{ formatCurrency(invoice.totalAmount) }}</td>
                <td>{{ formatCurrency(invoice.paidAmount) }}</td>
                <td>{{ formatCurrency(invoice.debtAmount) }}</td>
                <td>
                  <span class="status-badge" :class="'status-' + invoice.status?.toLowerCase()">
                    {{ invoice.status }}
                  </span>
                </td>
                <td>{{ formatDate(invoice.dueDate) }}</td>
                <td>
                  <button
                    v-if="invoice.debtAmount > 0"
                    @click="openPaymentModal(invoice)"
                    class="btn-small"
                  >
                    Thanh toán
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <!-- Payments Tab -->
      <section v-if="activeTab === 'payments'" class="tab-content">
        <div class="section-header">
          <h2>Lịch sử thanh toán</h2>
          <button @click="loadPayments" :disabled="isLoading" class="btn-primary">
            {{ isLoading ? 'Đang tải...' : 'Tải thanh toán' }}
          </button>
        </div>

        <div v-if="paymentError" class="error-message">{{ paymentError }}</div>

        <div v-if="payments.length > 0" class="payments-list">
          <table>
            <thead>
              <tr>
                <th>Hóa đơn</th>
                <th>Số tiền</th>
                <th>Phương thức</th>
                <th>Nhân viên thu</th>
                <th>Ngày thanh toán</th>
                <th>Ghi chú</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="payment in payments" :key="payment.id">
                <td>{{ payment.invoiceCode }}</td>
                <td>{{ formatCurrency(payment.amount) }}</td>
                <td>{{ payment.paymentMethod }}</td>
                <td>{{ payment.collectedBy }}</td>
                <td>{{ formatDate(payment.paidAt) }}</td>
                <td>{{ payment.note }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </div>

    <!-- Payment Modal -->
    <div v-if="showPaymentModal" class="modal-overlay" @click.self="closePaymentModal">
      <div class="modal">
        <div class="modal-header">
          <h2>Ghi nhận thanh toán</h2>
          <button @click="closePaymentModal" class="btn-close">✕</button>
        </div>

        <div class="modal-body">
          <div class="payment-info">
            <p><strong>Hóa đơn:</strong> {{ selectedInvoice?.invoiceCode }}</p>
            <p><strong>Còn nợ:</strong> {{ formatCurrency(selectedInvoice?.debtAmount) }}</p>
          </div>

          <form @submit.prevent="handleRecordPayment">
            <div class="form-group">
              <label>Số tiền thanh toán:</label>
              <input
                v-model.number="paymentForm.amount"
                type="number"
                step="0.01"
                :max="selectedInvoice?.debtAmount"
                required
              />
            </div>

            <div class="form-group">
              <label>Phương thức thanh toán:</label>
              <select v-model="paymentForm.paymentMethod" required>
                <option value="">-- Chọn phương thức --</option>
                <option value="Cash">Tiền mặt</option>
                <option value="Bank Transfer">Chuyển khoản</option>
                <option value="Check">Séc</option>
                <option value="Online">Thanh toán trực tuyến</option>
              </select>
            </div>

            <div class="form-group">
              <label>Nhân viên thu:</label>
              <input
                v-model="paymentForm.collectedBy"
                type="text"
                placeholder="Tên nhân viên"
              />
            </div>

            <div class="form-group">
              <label>Ghi chú:</label>
              <textarea
                v-model="paymentForm.note"
                placeholder="Ghi chú thêm (tùy chọn)"
                rows="3"
              ></textarea>
            </div>

            <div v-if="paymentFormError" class="error-message">{{ paymentFormError }}</div>

            <div class="modal-actions">
              <button type="button" @click="closePaymentModal" class="btn-secondary">
                Hủy
              </button>
              <button type="submit" :disabled="isSubmittingPayment" class="btn-primary">
                {{ isSubmittingPayment ? 'Đang xử lý...' : 'Ghi nhận thanh toán' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/authStore';
import api from '../api';

const authStore = useAuthStore();
const router = useRouter();

const activeTab = ref('invoices');
const isLoading = ref(false);
const isSubmittingPayment = ref(false);

const invoices = ref<any[]>([]);
const invoiceError = ref('');

const payments = ref<any[]>([]);
const paymentError = ref('');

const showPaymentModal = ref(false);
const selectedInvoice = ref<any>(null);
const paymentFormError = ref('');

const paymentForm = reactive({
  amount: 0,
  paymentMethod: '',
  collectedBy: authStore.user?.fullName || '',
  note: ''
});

const formatCurrency = (value: number) => {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND'
  }).format(value);
};

const formatDate = (date: string) => {
  return new Date(date).toLocaleDateString('vi-VN');
};

const loadInvoices = async () => {
  invoiceError.value = '';
  isLoading.value = true;
  try {
    const response = await api.get('/admin/invoices');
    invoices.value = response.data;
  } catch (err: any) {
    invoiceError.value = err.response?.data?.message || 'Không thể tải danh sách hóa đơn';
  } finally {
    isLoading.value = false;
  }
};

const loadPayments = async () => {
  paymentError.value = '';
  isLoading.value = true;
  try {
    const response = await api.get('/admin/payments');
    payments.value = response.data;
  } catch (err: any) {
    paymentError.value = err.response?.data?.message || 'Không thể tải lịch sử thanh toán';
  } finally {
    isLoading.value = false;
  }
};

const openPaymentModal = (invoice: any) => {
  selectedInvoice.value = invoice;
  paymentForm.amount = invoice.debtAmount;
  paymentFormError.value = '';
  showPaymentModal.value = true;
};

const closePaymentModal = () => {
  showPaymentModal.value = false;
  selectedInvoice.value = null;
  paymentFormError.value = '';
  paymentForm.amount = 0;
  paymentForm.paymentMethod = '';
  paymentForm.note = '';
};

const handleRecordPayment = async () => {
  paymentFormError.value = '';

  if (!paymentForm.amount || paymentForm.amount <= 0) {
    paymentFormError.value = 'Số tiền phải lớn hơn 0';
    return;
  }

  if (!paymentForm.paymentMethod) {
    paymentFormError.value = 'Vui lòng chọn phương thức thanh toán';
    return;
  }

  isSubmittingPayment.value = true;

  try {
    await api.post('/payments', {
      invoiceId: selectedInvoice.value.id,
      amount: paymentForm.amount,
      paymentMethod: paymentForm.paymentMethod,
      collectedBy: paymentForm.collectedBy,
      note: paymentForm.note,
      paidAt: new Date().toISOString()
    });

    closePaymentModal();
    await loadInvoices();
    await loadPayments();
  } catch (err: any) {
    paymentFormError.value = err.response?.data?.message || 'Ghi nhận thanh toán thất bại';
  } finally {
    isSubmittingPayment.value = false;
  }
};

const handleLogout = async () => {
  try {
    await authStore.logout();
    router.push('/login');
  } catch (err) {
    console.error('Logout error:', err);
    router.push('/login');
  }
};
</script>

<style scoped>
.admin-dashboard {
  min-height: 100vh;
  background-color: #f5f5f5;
}

.header {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 1.5rem 0;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.header-content {
  max-width: 1400px;
  margin: 0 auto;
  padding: 0 1rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.header h1 {
  margin: 0;
  font-size: 1.8rem;
}

.header-actions {
  display: flex;
  gap: 1rem;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.btn-logout {
  padding: 0.5rem 1rem;
  background-color: rgba(255, 255, 255, 0.2);
  color: white;
  border: 1px solid rgba(255, 255, 255, 0.5);
  border-radius: 4px;
  cursor: pointer;
  transition: all 0.3s;
}

.btn-logout:hover {
  background-color: rgba(255, 255, 255, 0.3);
}

.container {
  max-width: 1400px;
  margin: 0 auto;
  padding: 2rem 1rem;
}

.tabs {
  display: flex;
  gap: 1rem;
  margin-bottom: 2rem;
  border-bottom: 2px solid #e0e0e0;
}

.tab-btn {
  padding: 1rem 1.5rem;
  background: none;
  border: none;
  font-size: 1rem;
  font-weight: 600;
  color: #666;
  cursor: pointer;
  border-bottom: 3px solid transparent;
  transition: all 0.3s;
  margin-bottom: -2px;
}

.tab-btn.active {
  color: #667eea;
  border-bottom-color: #667eea;
}

.tab-btn:hover:not(.active) {
  color: #333;
}

.tab-content {
  background: white;
  padding: 2rem;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.section-header h2 {
  margin: 0;
  color: #333;
}

.btn-primary {
  padding: 0.75rem 1.5rem;
  background-color: #667eea;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-weight: 600;
  transition: background-color 0.3s;
}

.btn-primary:hover:not(:disabled) {
  background-color: #5568d3;
}

.btn-primary:disabled {
  background-color: #999;
  cursor: not-allowed;
}

.error-message {
  padding: 1rem;
  background-color: #fee;
  color: #c00;
  border-radius: 4px;
  margin-bottom: 1.5rem;
}

.invoices-list,
.payments-list {
  overflow-x: auto;
}

table {
  width: 100%;
  border-collapse: collapse;
}

thead {
  background-color: #f9f9f9;
}

th {
  padding: 1rem;
  text-align: left;
  font-weight: 600;
  color: #333;
  border-bottom: 2px solid #e0e0e0;
  font-size: 0.9rem;
}

td {
  padding: 1rem;
  border-bottom: 1px solid #e0e0e0;
  color: #666;
}

tbody tr:hover {
  background-color: #f9f9f9;
}

.status-badge {
  display: inline-block;
  padding: 0.4rem 0.8rem;
  border-radius: 12px;
  font-size: 0.85rem;
  font-weight: 600;
}

.status-unpaid {
  background-color: #fee;
  color: #c33;
}

.status-partial {
  background-color: #fef3cd;
  color: #856404;
}

.status-paid {
  background-color: #efe;
  color: #060;
}

.status-overdue {
  background-color: #ffe0e0;
  color: #900;
}

.btn-small {
  padding: 0.4rem 0.8rem;
  background-color: #667eea;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.85rem;
  transition: background-color 0.3s;
}

.btn-small:hover {
  background-color: #5568d3;
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal {
  background: white;
  border-radius: 8px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.3);
  max-width: 500px;
  width: 100%;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem;
  border-bottom: 1px solid #e0e0e0;
}

.modal-header h2 {
  margin: 0;
  color: #333;
}

.btn-close {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  color: #999;
}

.btn-close:hover {
  color: #333;
}

.modal-body {
  padding: 1.5rem;
}

.payment-info {
  background-color: #f9f9f9;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1.5rem;
}

.payment-info p {
  margin: 0.5rem 0;
  color: #333;
}

.form-group {
  margin-bottom: 1.5rem;
}

label {
  display: block;
  margin-bottom: 0.5rem;
  color: #333;
  font-weight: 600;
}

input,
select,
textarea {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
  box-sizing: border-box;
  font-family: inherit;
}

input:focus,
select:focus,
textarea:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.modal-actions {
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
  margin-top: 2rem;
  padding-top: 1.5rem;
  border-top: 1px solid #e0e0e0;
}

.btn-secondary {
  padding: 0.75rem 1.5rem;
  background-color: #e0e0e0;
  color: #333;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-weight: 600;
  transition: background-color 0.3s;
}

.btn-secondary:hover {
  background-color: #d0d0d0;
}
</style>
