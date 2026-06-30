import axios, { AxiosError, AxiosInstance } from 'axios';

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || 'http://service3-paymentandreport.somee.com/api/payment-report';

const api: AxiosInstance = axios.create({
  baseURL: apiBaseUrl,
  headers: {
    'Content-Type': 'application/json'
  }
});

// Flag to prevent multiple refresh token requests
let isRefreshing = false;
let failedQueue: Array<{
  onSuccess: (token: string) => void;
  onError: (error: any) => void;
}> = [];

const processQueue = (error: any, token: string | null = null) => {
  failedQueue.forEach(prom => {
    if (error) {
      prom.onError(error);
    } else {
      prom.onSuccess(token || '');
    }
  });
  
  isRefreshing = false;
  failedQueue = [];
};

// Request interceptor: Add token to headers
api.interceptors.request.use(
  (config) => {
    const authData = localStorage.getItem('auth');
    if (authData) {
      try {
        const { accessToken } = JSON.parse(authData);
        if (accessToken) {
          config.headers.Authorization = `Bearer ${accessToken}`;
        }
      } catch (e) {
        // Invalid storage data
      }
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor: Handle 401 and refresh token
api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as any;

    // If error is 401 and not already retried
    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        // Queue the request while refreshing
        return new Promise((onSuccess, onError) => {
          failedQueue.push({ onSuccess, onError });
        })
          .then((token) => {
            originalRequest.headers.Authorization = `Bearer ${token}`;
            return api(originalRequest);
          })
          .catch((err) => {
            return Promise.reject(err);
          });
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        const authData = localStorage.getItem('auth');
        if (!authData) {
          throw new Error('No auth data');
        }

        const { refreshToken } = JSON.parse(authData);
        if (!refreshToken) {
          throw new Error('No refresh token');
        }

        // Call refresh token endpoint
        const response = await axios.post<{
          accessToken: string;
          refreshToken: string;
        }>(
          `${apiBaseUrl}/auth/refresh-token`,
          { refreshToken }
        );

        const { accessToken: newAccessToken, refreshToken: newRefreshToken } = response.data;

        // Update storage
        localStorage.setItem(
          'auth',
          JSON.stringify({
            accessToken: newAccessToken,
            refreshToken: newRefreshToken,
            user: JSON.parse(authData).user
          })
        );

        // Update original request
        originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
        processQueue(null, newAccessToken);

        return api(originalRequest);
      } catch (refreshError) {
        processQueue(refreshError, null);

        // Clear auth and redirect to login
        localStorage.removeItem('auth');
        window.location.href = '/login';

        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

export default api;
