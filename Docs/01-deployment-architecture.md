# Deployment Architecture

## 1. Mục tiêu

Tài liệu này chốt kiến trúc triển khai cho hệ thống quản lý khóa học và học viên trung tâm.

Hệ thống gồm:

- Frontend VueJS
- Ocelot API Gateway
- Course & Schedule Service
- Student & Attendance Service
- Payment & Report/Auth Service
- Azure SQL Database cho từng service

Nguyên tắc triển khai:

- Frontend deploy lên Vercel.
- Backend deploy lên Render.
- API Gateway deploy lên Render.
- Mỗi backend service là một Render Web Service riêng.
- Mỗi backend service có một Azure SQL Database riêng.
- Không tách read/write database.
- Không dùng chung database giữa các service.
- Không lưu secret trong Git.
- Frontend chỉ gọi API Gateway, không gọi trực tiếp các service khi deploy chính thức.

---

## 2. Sơ đồ triển khai tổng thể

```txt
Vercel
└── VueJS Frontend
        |
        | HTTPS
        v
Render
└── Ocelot API Gateway
        |
        | HTTPS
        |-- Course & Schedule Service
        |       └── Azure SQL: CourseScheduleDb
        |
        |-- Student & Attendance Service
        |       └── Azure SQL: StudentAttendanceDb
        |
        |-- Payment & Report/Auth Service
                └── Azure SQL: PaymentReportDb
```

---

## 3. Thành phần deploy

| Thành phần | Nền tảng deploy | Ghi chú |
|---|---|---|
| VueJS Frontend | Vercel | Static frontend |
| Ocelot API Gateway | Render Web Service | Điểm vào duy nhất cho frontend |
| Course & Schedule Service | Render Web Service | ASP.NET Core Web API |
| Student & Attendance Service | Render Web Service | ASP.NET Core Web API |
| Payment & Report/Auth Service | Render Web Service | ASP.NET Core Web API, phát hành JWT |
| CourseScheduleDb | Azure SQL Database | Database riêng cho Course Service |
| StudentAttendanceDb | Azure SQL Database | Database riêng cho Student Service |
| PaymentReportDb | Azure SQL Database | Database riêng cho Payment/Auth Service |

---

## 4. URL deploy đề xuất

Tên URL có thể thay đổi theo tên service trên Render/Vercel.

```txt
Frontend:
https://training-center.vercel.app

Gateway:
https://training-center-gateway.onrender.com

Course & Schedule Service:
https://course-schedule-service.onrender.com

Student & Attendance Service:
https://student-attendance-service.onrender.com

Payment & Report/Auth Service:
https://payment-report-service.onrender.com
```

---

## 5. Luồng request từ frontend

Frontend chỉ cấu hình một base URL:

```env
VITE_API_BASE_URL=https://training-center-gateway.onrender.com
```

Ví dụ frontend gọi:

```http
GET https://training-center-gateway.onrender.com/api/course-schedule/student/classes/open
```

Gateway route tới:

```http
GET https://course-schedule-service.onrender.com/api/course-schedule/student/classes/open
```

Ví dụ frontend gọi:

```http
POST https://training-center-gateway.onrender.com/api/payment-report/auth/login
```

Gateway route tới:

```http
POST https://payment-report-service.onrender.com/api/payment-report/auth/login
```

---

## 6. API prefix

| Service | API Prefix |
|---|---|
| Course & Schedule Service | `/api/course-schedule` |
| Student & Attendance Service | `/api/student-attendance` |
| Payment & Report/Auth Service | `/api/payment-report` |

Gateway route theo prefix:

```txt
/api/course-schedule/...      -> Course & Schedule Service
/api/student-attendance/...   -> Student & Attendance Service
/api/payment-report/...       -> Payment & Report/Auth Service
```

---

## 7. JWT và phân quyền

Payment & Report/Auth Service chịu trách nhiệm:

- Login
- Quản lý tài khoản
- Phát hành JWT
- Refresh token nếu có
- Reset password
- Change password

Course & Schedule Service và Student & Attendance Service:

- Không phát hành JWT.
- Chỉ verify JWT.
- Kiểm tra role từ claim `role`.

API Gateway:

- Có thể verify JWT trước khi forward request.
- Dù Gateway verify JWT, từng service vẫn nên verify lại JWT để an toàn.

JWT settings phải giống nhau ở Gateway và 3 backend service:

```txt
Jwt__Issuer
Jwt__Audience
Jwt__SecretKey
```

Role chuẩn:

```txt
Admin
Teacher
Student
InternalService
```

---

## 8. Internal service-to-service

Các service gọi nhau qua Internal API, không truy cập database của nhau.

Ví dụ:

```txt
Student & Attendance Service
    -> gọi Course & Schedule Service để kiểm tra lớp còn chỗ

Student & Attendance Service
    -> gọi Payment & Report/Auth Service để tạo invoice sau khi approve enrollment

Payment & Report/Auth Service
    -> gọi Student & Attendance Service để lấy thông tin enrollment/report nếu cần
```

Internal API nên được bảo vệ bằng một trong hai cách:

### Cách ưu tiên

Dùng JWT có role:

```txt
InternalService
```

### Cách đơn giản hơn cho đồ án

Dùng header:

```http
X-Internal-Api-Key: <internal-secret>
```

Cách nào được chọn thì phải dùng thống nhất cho cả 3 service.

---

## 9. Database

Chốt database:

```txt
Course & Schedule Service      -> CourseScheduleDb
Student & Attendance Service   -> StudentAttendanceDb
Payment & Report/Auth Service  -> PaymentReportDb
```

Không làm:

```txt
TrainingCenterDb dùng chung cho cả 3 service
CourseScheduleReadDb / CourseScheduleWriteDb
StudentAttendanceReadDb / StudentAttendanceWriteDb
PaymentReportReadDb / PaymentReportWriteDb
```

Nguyên tắc:

- Mỗi service chỉ kết nối database của chính nó.
- Không service nào query trực tiếp database của service khác.
- Khi cần dữ liệu của service khác thì gọi Internal API.

---

## 10. CORS

Production chỉ mở CORS cho frontend Vercel:

```txt
https://training-center.vercel.app
```

Development có thể mở thêm:

```txt
http://localhost:5173
```

Không dùng `AllowAnyOrigin()` trong production.

---

## 11. Secret và environment variables

Không commit secret vào Git:

- JWT secret
- Connection string Azure SQL
- Internal API key
- Password database
- Production `.env`
- Production `appsettings`

Chỉ commit file mẫu:

```txt
.env.example
appsettings.Example.json
```

Secret thật lưu tại:

- Render Environment Variables
- Vercel Environment Variables
- Azure Portal
