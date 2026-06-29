# Environment Variables

## 1. Mục tiêu

Tài liệu này chốt tên biến môi trường cho Render, Vercel và các service.

Nguyên tắc:

- Không lưu secret trong Git.
- Secret thật chỉ lưu trên Render/Vercel/Azure.
- Tất cả backend dùng cùng JWT issuer, audience, secret.
- Mỗi service có connection string riêng.
- Frontend chỉ biết Gateway URL.

---

## 2. Quy ước đặt tên env cho ASP.NET Core

ASP.NET Core đọc nested config bằng dấu `__`.

Ví dụ:

```txt
ConnectionStrings__DefaultConnection
Jwt__Issuer
Jwt__Audience
Jwt__SecretKey
Cors__AllowedOrigins__0
ExternalServices__CourseScheduleBaseUrl
```

Tương ứng với JSON:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "..."
  },
  "Jwt": {
    "Issuer": "...",
    "Audience": "...",
    "SecretKey": "..."
  },
  "Cors": {
    "AllowedOrigins": [
      "..."
    ]
  }
}
```

---

## 3. Env chung cho backend

Các backend service và Gateway cần dùng chung:

```txt
ASPNETCORE_ENVIRONMENT=Production
Jwt__Issuer=TrainingCenter
Jwt__Audience=TrainingCenterUsers
Jwt__SecretKey=<same-secret-key>
```

Yêu cầu:

- `Jwt__SecretKey` phải giống nhau ở Gateway, Course Service, Student Service, Payment Service.
- Payment Service dùng secret để phát hành token.
- Gateway, Course Service, Student Service dùng secret để verify token.
- Không commit secret này vào Git.

---

## 4. CORS env

Production:

```txt
Cors__AllowedOrigins__0=https://training-center.vercel.app
```

Development:

```txt
Cors__AllowedOrigins__1=http://localhost:5173
```

Nếu có thêm domain preview Vercel, thêm:

```txt
Cors__AllowedOrigins__2=https://your-preview-domain.vercel.app
```

Không dùng `AllowAnyOrigin()` trong production.

---

## 5. Course & Schedule Service env

Render service:

```txt
course-schedule-service
```

Env cần có:

```txt
ASPNETCORE_ENVIRONMENT=Production

ConnectionStrings__DefaultConnection=<Azure SQL CourseScheduleDb connection string>

Jwt__Issuer=TrainingCenter
Jwt__Audience=TrainingCenterUsers
Jwt__SecretKey=<same-secret-key>

Cors__AllowedOrigins__0=https://training-center.vercel.app
Cors__AllowedOrigins__1=http://localhost:5173

ExternalServices__PaymentReportBaseUrl=https://payment-report-service.onrender.com
```

Nếu Course Service cần gọi Student Service cho report hoặc danh sách học viên lớp:

```txt
ExternalServices__StudentAttendanceBaseUrl=https://student-attendance-service.onrender.com
```

---

## 6. Student & Attendance Service env

Render service:

```txt
student-attendance-service
```

Env cần có:

```txt
ASPNETCORE_ENVIRONMENT=Production

ConnectionStrings__DefaultConnection=<Azure SQL StudentAttendanceDb connection string>

Jwt__Issuer=TrainingCenter
Jwt__Audience=TrainingCenterUsers
Jwt__SecretKey=<same-secret-key>

Cors__AllowedOrigins__0=https://training-center.vercel.app
Cors__AllowedOrigins__1=http://localhost:5173

ExternalServices__CourseScheduleBaseUrl=https://course-schedule-service.onrender.com
ExternalServices__PaymentReportBaseUrl=https://payment-report-service.onrender.com
```

Student Service cần gọi:

- Course Service để kiểm tra lớp, lịch, sĩ số.
- Payment/Auth Service để tạo tài khoản Student, tạo invoice sau approve enrollment.

---

## 7. Payment & Report/Auth Service env

Render service:

```txt
payment-report-service
```

Env cần có:

```txt
ASPNETCORE_ENVIRONMENT=Production

ConnectionStrings__DefaultConnection=<Azure SQL PaymentReportDb connection string>

Jwt__Issuer=TrainingCenter
Jwt__Audience=TrainingCenterUsers
Jwt__SecretKey=<same-secret-key>

Cors__AllowedOrigins__0=https://training-center.vercel.app
Cors__AllowedOrigins__1=http://localhost:5173

ExternalServices__CourseScheduleBaseUrl=https://course-schedule-service.onrender.com
ExternalServices__StudentAttendanceBaseUrl=https://student-attendance-service.onrender.com
```

Payment/Auth Service phát hành JWT và quản lý tài khoản.

---

## 8. API Gateway env

Render service:

```txt
training-center-gateway
```

Env cần có:

```txt
ASPNETCORE_ENVIRONMENT=Production

Jwt__Issuer=TrainingCenter
Jwt__Audience=TrainingCenterUsers
Jwt__SecretKey=<same-secret-key>

Cors__AllowedOrigins__0=https://training-center.vercel.app
Cors__AllowedOrigins__1=http://localhost:5173

ServiceUrls__CourseSchedule=https://course-schedule-service.onrender.com
ServiceUrls__StudentAttendance=https://student-attendance-service.onrender.com
ServiceUrls__PaymentReport=https://payment-report-service.onrender.com
```

Nếu Gateway dùng `ocelot.json` hard-code URL thì vẫn nên lưu URL trong env để dễ thay đổi về sau.

---

## 9. Frontend Vercel env

Vercel project:

```txt
training-center-frontend
```

Env cần có:

```txt
VITE_API_BASE_URL=https://training-center-gateway.onrender.com
```

Không lưu trong frontend:

```txt
JWT secret
Database connection string
Internal service API key
Admin password
```

---

## 10. Internal service authentication env

Nếu dùng Internal API Key:

```txt
InternalAuth__ApiKey=<internal-secret>
```

Tất cả service cần gọi/nhận internal request phải dùng cùng key.

Nếu dùng JWT role `InternalService`, cần thêm cơ chế lấy internal token từ Payment Service hoặc tự tạo token với cùng secret theo chuẩn đã thống nhất.

Khuyến nghị cho đồ án:

```txt
Dùng X-Internal-Api-Key cho đơn giản
```

Header:

```http
X-Internal-Api-Key: <internal-secret>
```

Endpoint `/internal/...` chỉ chấp nhận request có key đúng.

---

## 11. File appsettings.Example.json

Mỗi service nên có file mẫu:

```txt
appsettings.Example.json
```

Nội dung mẫu:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "PUT_CONNECTION_STRING_HERE"
  },
  "Jwt": {
    "Issuer": "TrainingCenter",
    "Audience": "TrainingCenterUsers",
    "SecretKey": "PUT_SECRET_KEY_HERE"
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5173"
    ]
  },
  "ExternalServices": {
    "CourseScheduleBaseUrl": "https://course-schedule-service.onrender.com",
    "StudentAttendanceBaseUrl": "https://student-attendance-service.onrender.com",
    "PaymentReportBaseUrl": "https://payment-report-service.onrender.com"
  }
}
```

Không commit `appsettings.Production.json` chứa secret thật.

---

## 12. File .env.example cho frontend

```env
VITE_API_BASE_URL=https://training-center-gateway.onrender.com
```

Không commit:

```txt
.env
.env.local
.env.production
```

---

## 13. Checklist env trước deploy

Trước khi bấm deploy, kiểm tra:

```txt
Mỗi service có ConnectionStrings__DefaultConnection chưa?
JWT secret giống nhau ở 4 backend app chưa?
CORS đã mở domain Vercel chưa?
Gateway đã có URL của 3 service chưa?
Frontend đã có VITE_API_BASE_URL chưa?
Không có secret trong Git chưa?
```
