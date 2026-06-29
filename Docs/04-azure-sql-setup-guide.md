# Azure SQL Setup Guide

## 1. Mục tiêu

Tài liệu này hướng dẫn chuẩn bị Azure SQL Database cho 3 backend service.

Chốt database:

```txt
Course & Schedule Service      -> CourseScheduleDb
Student & Attendance Service   -> StudentAttendanceDb
Payment & Report/Auth Service  -> PaymentReportDb
```

Không dùng chung database.

Không tách read/write database.

---

## 2. Kiến trúc database

```txt
Azure SQL Server
│
├── CourseScheduleDb
├── StudentAttendanceDb
└── PaymentReportDb
```

Mỗi service chỉ kết nối database của chính nó:

| Service | Database |
|---|---|
| Course & Schedule Service | `CourseScheduleDb` |
| Student & Attendance Service | `StudentAttendanceDb` |
| Payment & Report/Auth Service | `PaymentReportDb` |

---

## 3. Tạo Azure SQL Server

Trên Azure Portal:

1. Vào **SQL databases**.
2. Chọn **Create**.
3. Tạo hoặc chọn Resource Group.
4. Tạo SQL Server mới.
5. Đặt Server name, ví dụ:

```txt
training-center-sql-server
```

6. Chọn region gần người dùng.
7. Tạo tài khoản admin cho SQL Server.
8. Ghi lại:

```txt
Server name
Admin username
Admin password
```

Không commit thông tin này lên Git.

---

## 4. Tạo 3 database

Tạo lần lượt:

```txt
CourseScheduleDb
StudentAttendanceDb
PaymentReportDb
```

Gợi ý cho đồ án:

- Dùng tier tiết kiệm chi phí.
- Không cần cấu hình quá cao.
- Nếu là demo, ưu tiên cấu hình rẻ nhất có thể.

---

## 5. Firewall cho Azure SQL

Azure SQL cần cho phép IP của app backend truy cập.

Lưu ý:

- Render outbound IP có thể thay đổi tùy plan/cấu hình.
- Nếu không có static outbound IP, việc cấu hình firewall chính xác có thể khó.
- Với đồ án/demo, có thể mở firewall tạm thời rộng hơn, nhưng cần hiểu rủi ro bảo mật.
- Production thật nên dùng giải pháp bảo mật hơn như static outbound IP/VNet/private networking tùy nền tảng.

Cách cấu hình cơ bản:

1. Vào Azure SQL Server.
2. Chọn **Networking** hoặc **Firewalls and virtual networks**.
3. Thêm firewall rule cho IP cần truy cập.
4. Lưu lại.

Nếu test local bằng máy cá nhân, thêm IP public của máy hiện tại vào firewall.

---

## 6. Lấy connection string

Trong Azure Portal:

1. Vào từng database.
2. Chọn **Connection strings**.
3. Chọn tab **ADO.NET**.
4. Copy connection string.
5. Thay username/password thật.

Ví dụ format:

```txt
Server=tcp:<server-name>.database.windows.net,1433;
Initial Catalog=CourseScheduleDb;
Persist Security Info=False;
User ID=<username>;
Password=<password>;
MultipleActiveResultSets=False;
Encrypt=True;
TrustServerCertificate=False;
Connection Timeout=30;
```

Khi đưa vào Render env, có thể viết một dòng:

```txt
ConnectionStrings__DefaultConnection=Server=tcp:<server-name>.database.windows.net,1433;Initial Catalog=CourseScheduleDb;User ID=<username>;Password=<password>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

---

## 7. Cấu hình connection string cho từng service

### Course & Schedule Service

```txt
ConnectionStrings__DefaultConnection=<connection-string-to-CourseScheduleDb>
```

### Student & Attendance Service

```txt
ConnectionStrings__DefaultConnection=<connection-string-to-StudentAttendanceDb>
```

### Payment & Report/Auth Service

```txt
ConnectionStrings__DefaultConnection=<connection-string-to-PaymentReportDb>
```

Không cấu hình Course Service trỏ sang `StudentAttendanceDb`.

Không cấu hình Student Service trỏ sang `PaymentReportDb` để query trực tiếp.

---

## 8. Migration database

Mỗi service tự chạy migration cho database của mình.

Ví dụ Course Service:

```bash
cd services/course-schedule-service
dotnet ef database update --project CourseSchedule.Infrastructure --startup-project CourseSchedule.Api
```

Student Service:

```bash
cd services/student-attendance-service
dotnet ef database update --project StudentAttendance.Infrastructure --startup-project StudentAttendance.Api
```

Payment Service:

```bash
cd services/payment-report-service
dotnet ef database update --project PaymentReport.Infrastructure --startup-project PaymentReport.Api
```

Tên project có thể thay đổi theo code thật của nhóm.

Nếu nhóm dùng Docker deploy, có 2 cách:

### Cách 1: Chạy migration từ máy local

Máy local kết nối Azure SQL, chạy `dotnet ef database update`.

### Cách 2: App tự migrate khi start

Trong `Program.cs`, app tự gọi migrate khi start.

Cách này tiện cho demo nhưng cần cẩn thận ở production thật.

---

## 9. Seed data

Payment & Report/Auth Service cần seed ít nhất một tài khoản Admin.

Ví dụ:

```txt
Username: admin@center.com
Password: Admin@123456
Role: Admin
MustChangePassword: true
```

Không commit mật khẩu thật nếu là production.

Seed data nên đảm bảo:

```txt
Có Admin để login
Có một vài course/class mẫu nếu cần demo
Có teacher/student mẫu nếu cần demo
```

---

## 10. Kiểm tra kết nối database

Sau khi deploy backend, mở Swagger từng service:

```txt
https://course-schedule-service.onrender.com/swagger
https://student-attendance-service.onrender.com/swagger
https://payment-report-service.onrender.com/swagger
```

Test API đơn giản:

```http
GET /health
```

Hoặc API list:

```http
GET /api/course-schedule/admin/courses
GET /api/student-attendance/admin/students
GET /api/payment-report/admin/users
```

Nếu lỗi database, kiểm tra Render logs.

---

## 11. Lỗi thường gặp

### Login failed for user

Kiểm tra:

```txt
Username/password trong connection string
SQL Authentication đã bật chưa
Database user có quyền chưa
```

### Cannot open server requested by the login

Kiểm tra:

```txt
Firewall Azure SQL
Server name
Database name
```

### Timeout khi connect

Kiểm tra:

```txt
Firewall
Network
Connection string
Render service có outbound network không
```

### SSL/Encrypt error

Connection string nên có:

```txt
Encrypt=True;TrustServerCertificate=False;
```

Nếu gặp lỗi certificate khi demo nhanh có thể tạm dùng:

```txt
TrustServerCertificate=True
```

Nhưng production nên dùng cấu hình bảo mật đúng.

---

## 12. Quy tắc bảo mật database

Không commit:

```txt
Connection string thật
SQL username/password
Azure server admin password
```

Chỉ lưu secret ở:

```txt
Render Environment Variables
Azure Portal
```

Nếu cần file mẫu, dùng:

```txt
appsettings.Example.json
```

Ví dụ:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "PUT_AZURE_SQL_CONNECTION_STRING_HERE"
  }
}
```
