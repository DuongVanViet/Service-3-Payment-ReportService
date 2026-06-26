<template>
  <div class="login-container">
    <div class="login-box">
      <h1>Login</h1>

      <form @submit.prevent="handleLogin">
        <div class="form-group">
          <label>Username</label>
          <input
            v-model="credentials.username"
            type="text"
            placeholder="admin@training.local"
            required
          />
        </div>

        <div class="form-group">
          <label>Password</label>
          <input
            v-model="credentials.password"
            type="password"
            placeholder="Admin@123"
            required
          />
        </div>

        <button type="submit" :disabled="authStore.isLoading" class="btn-login">
          {{ authStore.isLoading ? 'Logging in...' : 'Login' }}
        </button>
      </form>

      <p v-if="authStore.error" class="error">{{ authStore.error }}</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/authStore';

const authStore = useAuthStore();
const router = useRouter();

const credentials = reactive({
  username: 'admin@training.local',
  password: 'Admin@123'
});

const handleLogin = async () => {
  try {
    const response = await authStore.login(credentials.username, credentials.password);

    // If user must change password, redirect to change password page
    if (response.user.mustChangePassword) {
      router.push('/change-password');
    } else {
      // Otherwise go to dashboard
      router.push('/dashboard');
    }
  } catch (err) {
    // Error is already set in authStore
  }
};
</script>

<style scoped>
.login-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.login-box {
  background: white;
  padding: 2rem;
  border-radius: 8px;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.2);
  width: 100%;
  max-width: 400px;
}

h1 {
  color: #333;
  text-align: center;
  margin-bottom: 1.5rem;
}

.form-group {
  margin-bottom: 1rem;
}

label {
  display: block;
  margin-bottom: 0.5rem;
  color: #333;
  font-weight: 500;
}

input {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
  box-sizing: border-box;
}

input:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.btn-login {
  width: 100%;
  padding: 0.75rem;
  background-color: #667eea;
  color: white;
  border: none;
  border-radius: 4px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  margin-top: 1rem;
  transition: background-color 0.3s;
}

.btn-login:hover:not(:disabled) {
  background-color: #5568d3;
}

.btn-login:disabled {
  background-color: #999;
  cursor: not-allowed;
}

.error {
  color: #c00;
  text-align: center;
  margin-top: 1rem;
  padding: 0.75rem;
  background-color: #fee;
  border-radius: 4px;
}
</style>
