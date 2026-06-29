# Deployment Checklist

## 1. Mục tiêu

Tài liệu này dùng để kiểm tra toàn bộ hệ thống trước khi demo hoặc nộp bài.

Hệ thống cần kiểm tra:

- Frontend Vercel
- API Gateway Render
- 3 backend service Render
- 3 Azure SQL database
- JWT
- CORS
- Service-to-service communication
- Core business flow

---

## 2. Checklist trước khi deploy

### Git

```txt
Code mới nhất đã merge vào branch deploy
Không còn file secret trong Git
Dockerfile của từng service đã có
Frontend build local thành công
Backend build local thành công
```

### Database

```txt
CourseScheduleDb đã tạo
StudentAttendanceDb đã tạo
PaymentReportDb đã tạo
Connection string đã lấy đúng
Azure SQL firewall đã cấu hình
Migration đã chạy
Seed Admin account đã có
```

### Env

```txt
Render env của Course Service đã đủ
Render env của Student Service đã đủ
Render env của Payment Service đã đủ
Render env của Gateway đã đủ
Vercel env VITE_API_BASE_URL đã đủ
JWT issuer/audience/secret giống nhau ở backend
CORS mở đúng domain Vercel
```

---

## 3. Checklist Render backend

### Course & Schedule Service

```txt
Render service deploy thành công
Swagger mở được
Kết nối CourseScheduleDb thành công
JWT verify được
CORS không lỗi
```

Test URL:

```txt
https://course-schedule-service.onrender.com/swagger
https://course-schedule-service.onrender.com/health
```

### Student & Attendance Service

```txt
Render service deploy thành công
Swagger mở được
Kết nối StudentAttendanceDb thành công
JWT verify được
Gọi được Course Service
Gọi được Payment/Auth Service
CORS không lỗi
```

Test URL:

```txt
https://student-attendance-service.onrender.com/swagger
https://student-attendance-service.onrender.com/health
```

### Payment & Report/Auth Service

```txt
Render service deploy thành công
Swagger mở được
Kết nối PaymentReportDb thành công
Login trả JWT
Tạo tài khoản được
Tạo invoice/payment được
CORS không lỗi
```

Test URL:

```txt
https://payment-report-service.onrender.com/swagger
https://payment-report-service.onrender.com/health
```

### API Gateway

```txt
Gateway deploy thành công
Gateway route được tới Course Service
Gateway route được tới Student Service
Gateway route được tới Payment Service
Gateway verify JWT nếu có cấu hình
CORS mở cho frontend Vercel
```

Test URL:

```txt
https://training-center-gateway.onrender.com/api/course-schedule/health
https://training-center-gateway.onrender.com/api/student-attendance/health
https://training-center-gateway.onrender.com/api/payment-report/health
```

---

## 4. Checklist Vercel frontend

```txt
Vercel deploy thành công
Trang login mở được
Frontend gọi đúng Gateway URL
Không gọi trực tiếp 3 backend service
Không bị lỗi CORS
Route guard hoạt động
Sidebar theo role hoạt động
```

Test URL:

```txt
https://training-center.vercel.app
```

Kiểm tra Network tab trong browser:

```txt
Request phải đi tới https://training-center-gateway.onrender.com
Không có request đi trực tiếp tới course-schedule-service.onrender.com
Không có request đi trực tiếp tới student-attendance-service.onrender.com
Không có request đi trực tiếp tới payment-report-service.onrender.com
```

---

## 5. Checklist JWT

### Login

```http
POST /api/payment-report/auth/login
```

Kết quả cần có:

```txt
Trả về accessToken
Có userId hoặc sub
Có role
Có relatedEntityId nếu là Teacher/Student
Có mustChangePassword
```

### Verify ở Course Service

Dùng token Admin/Teacher/Student gọi API Course tương ứng.

Kết quả:

```txt
Token hợp lệ -> truy cập được API đúng role
Token sai role -> 403
Không có token -> 401
Token sai secret -> 401
```

### Verify ở Student Service

Dùng token Admin/Teacher/Student gọi API Student/Attendance tương ứng.

Kết quả:

```txt
Token hợp lệ -> truy cập được API đúng role
Token sai role -> 403
Không có token -> 401
Token sai secret -> 401
```

---

## 6. Checklist CORS

Từ frontend Vercel gọi API Gateway:

```txt
Không có lỗi CORS trong browser console
Preflight OPTIONS thành công nếu có
Authorization header được gửi đúng
```

Nếu lỗi CORS, kiểm tra:

```txt
Gateway Cors__AllowedOrigins__0
Các service Cors__AllowedOrigins__0
Frontend domain có đúng không
Có dùng https không
```

---

## 7. Checklist business flow chính

### Flow 1: Admin tạo giáo viên

```txt
Admin login
Admin tạo teacher profile
Course Service lưu Teacher
Course Service gọi Payment/Auth tạo UserAccount role Teacher
Teacher login bằng mật khẩu tạm
Teacher bị yêu cầu đổi mật khẩu lần đầu
Teacher vào dashboard
```

### Flow 2: Admin tạo học viên

```txt
Admin login
Admin tạo student profile
Student Service lưu Student
Student Service gọi Payment/Auth tạo UserAccount role Student
Student login bằng mật khẩu tạm
Student bị yêu cầu đổi mật khẩu lần đầu
Student vào dashboard
```

### Flow 3: Student đăng ký lớp

```txt
Student login
Student xem lớp đang mở
Student đăng ký lớp
Student Service tạo Enrollment PendingApproval
Admin thấy đăng ký chờ duyệt
```

### Flow 4: Admin duyệt đăng ký

```txt
Admin duyệt enrollment
Student Service kiểm tra lớp qua Course Service
Student Service cập nhật Enrollment
Student Service gọi Course Service tăng sĩ số
Student Service gọi Payment Service tạo invoice
Student thấy học phí/công nợ
```

### Flow 5: Teacher điểm danh

```txt
Teacher login
Teacher xem lớp mình dạy
Teacher chọn session
Teacher điểm danh học viên
Student xem lịch sử điểm danh
Student thấy tỷ lệ chuyên cần
```

### Flow 6: Teacher nhập điểm

```txt
Teacher login
Teacher chọn lớp
Teacher nhập điểm giữa kỳ/cuối kỳ
Student xem kết quả
Admin xem báo cáo kết quả
```

### Flow 7: Admin ghi nhận thanh toán

```txt
Admin mở invoice
Admin ghi nhận payment
Payment Service cập nhật invoice
Student xem lịch sử thanh toán
Công nợ giảm
```

---

## 8. Checklist import/bulk

```txt
Import học viên preview được
Import học viên confirm được
Import lỗi hiển thị dòng lỗi
Bulk enroll nhiều học viên vào lớp được
Duyệt nhiều đăng ký nếu có làm
Import học phí nếu có làm
Import điểm nếu có làm
```

---

## 9. Checklist Gateway route

| Request Gateway | Kết quả mong muốn |
|---|---|
| `/api/course-schedule/...` | Route tới Course Service |
| `/api/student-attendance/...` | Route tới Student Service |
| `/api/payment-report/...` | Route tới Payment Service |

Nếu route lỗi:

```txt
Kiểm tra ocelot.json
Kiểm tra DownstreamHostAndPorts
Kiểm tra service Render URL
Kiểm tra service có đang sleep không
Kiểm tra HTTPS/HTTP
```

---

## 10. Checklist bảo mật

```txt
Không commit JWT secret
Không commit Azure SQL connection string
Không commit .env production
Không commit appsettings.Production.json chứa secret
CORS không AllowAnyOrigin trong production
Internal API không public tự do
Admin password demo không dùng password quá yếu
```

---

## 11. Checklist trước demo

```txt
Frontend mở được
Gateway mở được
3 Swagger mở được
Database có seed data
Có tài khoản Admin demo
Có ít nhất 1 course
Có ít nhất 1 teacher
Có ít nhất 1 class
Có ít nhất 1 student
Có thể chạy flow đăng ký -> duyệt -> invoice
Có thể chạy flow điểm danh
Có thể chạy flow nhập điểm
Có thể xem học phí/công nợ
```

---

## 12. Lệnh test nhanh bằng PowerShell

```powershell
Invoke-WebRequest https://course-schedule-service.onrender.com/swagger
Invoke-WebRequest https://student-attendance-service.onrender.com/swagger
Invoke-WebRequest https://payment-report-service.onrender.com/swagger
Invoke-WebRequest https://training-center-gateway.onrender.com/api/payment-report/health
```

---

## 13. Ghi nhận lỗi sau deploy

Nếu lỗi xảy ra, ghi lại:

```txt
Thời điểm lỗi
Service bị lỗi
Endpoint bị lỗi
HTTP status code
Request body
Response body
Log Render
Ảnh chụp browser console nếu lỗi frontend
```

Sau đó phân công đúng nhóm xử lý:

| Lỗi | Nhóm xử lý |
|---|---|
| Course/Class/Schedule | Nhóm Course |
| Student/Enrollment/Attendance/Result | Nhóm Student |
| Auth/Invoice/Payment/Report | Nhóm Payment |
| Gateway route/CORS | Nhóm Gateway/Lead |
| UI/Router/Axios | Nhóm Frontend |
| Azure SQL | Người phụ trách DB |
