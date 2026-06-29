# Vercel Frontend Deploy Guide

## 1. Mục tiêu

Tài liệu này hướng dẫn deploy frontend VueJS lên Vercel.

Frontend chỉ gọi API Gateway, không gọi trực tiếp 3 service backend trong môi trường deploy chính thức.

---

## 2. Cấu trúc repo liên quan

```txt
training-center-management/
└── frontend/
    └── vue-client/
        ├── src/
        ├── public/
        ├── package.json
        ├── vite.config.ts
        └── .env.example
```

---

## 3. Cấu hình deploy trên Vercel

| Mục | Giá trị |
|---|---|
| Framework | Vite |
| Root Directory | `frontend/vue-client` |
| Build Command | `npm run build` |
| Output Directory | `dist` |
| Install Command | `npm install` |

---

## 4. Environment variables trên Vercel

Frontend cần cấu hình biến môi trường:

```env
VITE_API_BASE_URL=https://training-center-gateway.onrender.com
```

Nếu domain Gateway thay đổi, chỉ sửa biến này trên Vercel.

Không commit file `.env.production` chứa URL thật nếu nhóm không muốn public cấu hình.

Có thể commit `.env.example`:

```env
VITE_API_BASE_URL=https://your-gateway-url.onrender.com
```

---

## 5. Quy trình deploy lần đầu

1. Vào Vercel Dashboard.
2. Chọn **Add New Project**.
3. Import GitHub repository `training-center-management`.
4. Ở phần Root Directory, chọn:

```txt
frontend/vue-client
```

5. Vercel nhận diện framework là Vite.
6. Cấu hình:

```txt
Build Command: npm run build
Output Directory: dist
```

7. Thêm Environment Variable:

```env
VITE_API_BASE_URL=https://training-center-gateway.onrender.com
```

8. Bấm Deploy.

---

## 6. Cấu hình Axios frontend

Frontend nên có file:

```txt
src/api/httpClient.ts
```

Ví dụ:

```ts
import axios from 'axios'

export const httpClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  timeout: 30000
})

httpClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken')

  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }

  return config
})
```

Mọi API module dùng chung `httpClient`.

---

## 7. API base URL trong các môi trường

### Local development qua Gateway local

```env
VITE_API_BASE_URL=http://localhost:7000
```

### Local development gọi backend qua Radmin nếu chưa có Gateway

```env
VITE_COURSE_API_BASE_URL=http://26.197.54.215:5201
VITE_STUDENT_API_BASE_URL=http://26.44.91.179:5202
VITE_PAYMENT_API_BASE_URL=http://26.222.155.205:5203
```

### Production/Staging

```env
VITE_API_BASE_URL=https://training-center-gateway.onrender.com
```

Khuyến nghị deploy chính thức chỉ dùng một biến:

```env
VITE_API_BASE_URL
```

---

## 8. CORS cần backend hỗ trợ

Frontend Vercel sẽ có domain dạng:

```txt
https://training-center.vercel.app
```

Backend/Gateway cần mở CORS cho domain này.

Nếu Vercel tạo preview domain cho từng pull request, ví dụ:

```txt
https://training-center-git-feature-login-team.vercel.app
```

thì có hai lựa chọn:

1. Chỉ test production domain chính.
2. Thêm preview domain vào CORS tạm thời khi cần.

Không dùng `AllowAnyOrigin()` trong production nếu có thể tránh.

---

## 9. Kiểm tra sau khi deploy

Sau khi deploy frontend, kiểm tra:

```txt
https://training-center.vercel.app
```

Checklist:

```txt
Trang login mở được
Login gọi đúng Gateway URL
Không bị CORS
Sau login lưu được accessToken
Router theo role hoạt động
Admin vào được /admin/dashboard
Teacher vào được /teacher/dashboard
Student vào được /student/dashboard
Khi token lỗi thì redirect về /login
```

---

## 10. Lỗi thường gặp

### Vercel build fail

Kiểm tra:

```txt
package.json có script build chưa
npm run build chạy được local chưa
Root Directory đúng chưa
Node version có phù hợp không
```

### Màn hình trắng sau deploy

Kiểm tra:

```txt
Console browser có lỗi không
Vue Router mode history có cấu hình fallback chưa
Asset path có lỗi không
```

Nếu lỗi route deep-link, tạo file:

```txt
frontend/vue-client/vercel.json
```

Nội dung:

```json
{
  "rewrites": [
    {
      "source": "/(.*)",
      "destination": "/index.html"
    }
  ]
}
```

### API gọi sai URL

Kiểm tra:

```txt
VITE_API_BASE_URL trên Vercel
Đã redeploy sau khi sửa env chưa
Frontend có dùng import.meta.env.VITE_API_BASE_URL không
```

### Lỗi CORS

Kiểm tra:

```txt
Gateway đã mở CORS cho domain Vercel chưa
Frontend có gọi Gateway không
Hay đang gọi trực tiếp service khác
```

---

## 11. Quy tắc không lưu secret frontend

Frontend không nên chứa secret thật.

Không lưu trong frontend:

```txt
JWT secret
Database connection string
Internal API key
Admin password
```

Biến `VITE_API_BASE_URL` không phải secret.

Mọi secret phải nằm ở backend Render env hoặc Azure.
