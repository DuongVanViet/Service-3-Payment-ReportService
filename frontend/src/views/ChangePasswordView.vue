<template>
  <div class="change-password-container">
    <div class="change-password-box">
      <h1>Thay đổi mật khẩu</h1>
      <p class="subtitle">Vui lòng đổi mật khẩu tạm thời để tiếp tục</p>

      <form @submit.prevent="handleChangePassword">
        <div class="form-group">
          <label for="currentPassword">Mật khẩu hiện tại:</label>
          <input
            id="currentPassword"
            v-model="form.currentPassword"
            type="password"
            placeholder="Nhập mật khẩu hiện tại"
            required
          />
        </div>

        <div class="form-group">
          <label for="newPassword">Mật khẩu mới:</label>
          <input
            id="newPassword"
            v-model="form.newPassword"
            type="password"
            placeholder="Nhập mật khẩu mới"
            required
          />
        </div>

        <div class="form-group">
          <label for="confirmPassword">Xác nhận mật khẩu:</label>
          <input
            id="confirmPassword"
            v-model="form.confirmPassword"
            type="password"
            placeholder="Xác nhận mật khẩu mới"
            required
          />
        </div>

        <div v-if="error" class="error-message">{{ error }}</div>
        <div v-if="success" class="success-message">{{ success }}</div>

        <button type="submit" :disabled="isLoading" class="btn-primary">
          {{ isLoading ? 'Đang xử lý...' : 'Thay đổi mật khẩu' }}
        </button>
      </form>

      <div class="user-info">
        <p>Đăng nhập với: <strong>{{ authStore.user?.email }}</strong></p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/authStore';

const authStore = useAuthStore();
const router = useRouter();

const form = reactive({
  currentPassword: '',
  newPassword: '',
  confirmPassword: ''
});

const isLoading = ref(false);
const error = ref('');
const success = ref('');

const handleChangePassword = async () => {
  error.value = '';
  success.value = '';

  // Validation
  if (!form.currentPassword || !form.newPassword || !form.confirmPassword) {
    error.value = 'Vui lòng điền tất cả các trường';
    return;
  }

  if (form.newPassword !== form.confirmPassword) {
    error.value = 'Mật khẩu mới không khớp';
    return;
  }

  if (form.newPassword.length < 6) {
    error.value = 'Mật khẩu mới phải có ít nhất 6 ký tự';
    return;
  }

  isLoading.value = true;

  try {
    await authStore.changePassword(form.currentPassword, form.newPassword);
    success.value = 'Thay đổi mật khẩu thành công!';

    // Reset form
    form.currentPassword = '';
    form.newPassword = '';
    form.confirmPassword = '';

    // Redirect to dashboard after 1.5s
    setTimeout(() => {
      router.push('/dashboard');
    }, 1500);
  } catch (err: any) {
    error.value = err.response?.data?.message || 'Thay đổi mật khẩu thất bại';
  } finally {
    isLoading.value = false;
  }
};
</script>

<style scoped>
.change-password-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.change-password-box {
  background: white;
  padding: 2rem;
  border-radius: 8px;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.2);
  width: 100%;
  max-width: 400px;
}

h1 {
  color: #333;
  margin-bottom: 0.5rem;
  text-align: center;
}

.subtitle {
  color: #666;
  font-size: 0.9rem;
  text-align: center;
  margin-bottom: 1.5rem;
}

.form-group {
  margin-bottom: 1.5rem;
}

label {
  display: block;
  margin-bottom: 0.5rem;
  color: #333;
  font-weight: 500;
  font-size: 0.95rem;
}

input {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
  box-sizing: border-box;
  transition: border-color 0.3s;
}

input:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.error-message {
  padding: 0.75rem;
  margin-bottom: 1rem;
  background-color: #fee;
  color: #c00;
  border-radius: 4px;
  font-size: 0.9rem;
}

.success-message {
  padding: 0.75rem;
  margin-bottom: 1rem;
  background-color: #efe;
  color: #060;
  border-radius: 4px;
  font-size: 0.9rem;
}

.btn-primary {
  width: 100%;
  padding: 0.75rem;
  background-color: #667eea;
  color: white;
  border: none;
  border-radius: 4px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: background-color 0.3s;
}

.btn-primary:hover:not(:disabled) {
  background-color: #5568d3;
}

.btn-primary:disabled {
  background-color: #999;
  cursor: not-allowed;
}

.user-info {
  margin-top: 1.5rem;
  padding-top: 1.5rem;
  border-top: 1px solid #eee;
  text-align: center;
  color: #666;
  font-size: 0.9rem;
}

.user-info strong {
  color: #333;
}
</style>
