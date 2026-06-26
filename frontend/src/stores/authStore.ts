import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import api from '../api';

export interface User {
  id: number;
  username: string;
  email: string;
  fullName: string;
  phone: string;
  role: string;
  status: string;
  mustChangePassword: boolean;
  createdAt: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  user: User;
}

export const useAuthStore = defineStore('auth', () => {
  const user = ref<User | null>(null);
  const accessToken = ref<string>('');
  const refreshToken = ref<string>('');
  const isLoading = ref(false);
  const error = ref<string>('');

  const isAuthenticated = computed(() => !!accessToken.value);
  const isAdmin = computed(() => user.value?.role === 'Admin');
  const isStudent = computed(() => user.value?.role === 'Student');
  const isTeacher = computed(() => user.value?.role === 'Teacher');
  const mustChangePassword = computed(() => user.value?.mustChangePassword ?? false);

  // Load tokens from localStorage
  const loadFromStorage = () => {
    const stored = localStorage.getItem('auth');
    if (stored) {
      try {
        const { accessToken: token, refreshToken: refToken, user: userData } = JSON.parse(stored);
        accessToken.value = token;
        refreshToken.value = refToken;
        user.value = userData;
      } catch (e) {
        localStorage.removeItem('auth');
      }
    }
  };

  // Save to localStorage
  const saveToStorage = () => {
    localStorage.setItem(
      'auth',
      JSON.stringify({
        accessToken: accessToken.value,
        refreshToken: refreshToken.value,
        user: user.value
      })
    );
  };

  // Login
  const login = async (username: string, password: string) => {
    isLoading.value = true;
    error.value = '';
    try {
      const response = await api.post<LoginResponse>('/auth/login', { username, password });
      accessToken.value = response.data.accessToken;
      refreshToken.value = response.data.refreshToken;
      user.value = response.data.user;
      saveToStorage();
      return response.data;
    } catch (err: any) {
      error.value = err.response?.data?.message || 'Login failed';
      throw err;
    } finally {
      isLoading.value = false;
    }
  };

  // Refresh token
  const refreshAccessToken = async () => {
    if (!refreshToken.value) throw new Error('No refresh token');
    
    try {
      const response = await api.post<LoginResponse>('/auth/refresh-token', {
        refreshToken: refreshToken.value
      });
      accessToken.value = response.data.accessToken;
      refreshToken.value = response.data.refreshToken;
      saveToStorage();
      return response.data.accessToken;
    } catch (err: any) {
      logout();
      throw err;
    }
  };

  // Change password
  const changePassword = async (currentPassword: string, newPassword: string) => {
    isLoading.value = true;
    error.value = '';
    try {
      await api.post('/auth/change-password', { currentPassword, newPassword });
      if (user.value) {
        user.value.mustChangePassword = false;
        saveToStorage();
      }
    } catch (err: any) {
      error.value = err.response?.data?.message || 'Change password failed';
      throw err;
    } finally {
      isLoading.value = false;
    }
  };

  // Logout
  const logout = async () => {
    try {
      if (refreshToken.value) {
        await api.post('/auth/logout', { refreshToken: refreshToken.value });
      }
    } catch (err) {
      // Ignore logout errors
    } finally {
      accessToken.value = '';
      refreshToken.value = '';
      user.value = null;
      localStorage.removeItem('auth');
    }
  };

  // Get current user
  const getCurrentUser = async () => {
    if (!accessToken.value) return null;
    try {
      const response = await api.get<User>('/auth/me');
      user.value = response.data;
      saveToStorage();
      return response.data;
    } catch (err) {
      throw err;
    }
  };

  return {
    user,
    accessToken,
    refreshToken,
    isLoading,
    error,
    isAuthenticated,
    isAdmin,
    isStudent,
    isTeacher,
    mustChangePassword,
    loadFromStorage,
    login,
    logout,
    refreshAccessToken,
    changePassword,
    getCurrentUser
  };
});
