# Render Backend Deploy Guide

## 1. Mục tiêu

Tài liệu này hướng dẫn deploy các backend service ASP.NET Core và API Gateway lên Render.

Các app cần tạo trên Render:

```txt
course-schedule-service
student-attendance-service
payment-report-service
training-center-gateway
```

Mỗi app là một Render Web Service riêng.

---

## 2. Cấu trúc repo liên quan

```txt
training-center-management/
│
├── services/
│   ├── course-schedule-service/
│   ├── student-attendance-service/
│   └── payment-report-service/
│
├── gateway/
│   └── api-gateway/
│
└── docs/
```

---

## 3. Nguyên tắc deploy backend

- Mỗi service deploy độc lập.
- Mỗi service có Dockerfile riêng.
- Mỗi service có database riêng.
- Mỗi service cấu hình env riêng trên Render.
- Không lưu secret trong Git.
- Swagger nên bật trong staging/demo để kiểm tra nhanh.
- App phải bind đúng port Render cung cấp qua biến môi trường `PORT`.

---

## 4. Dockerfile chuẩn cho ASP.NET Core

Mỗi service cần có Dockerfile riêng trong thư mục service.

Ví dụ với Course & Schedule Service:

```txt
services/course-schedule-service/Dockerfile
```

Dockerfile mẫu:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

ENTRYPOINT ["dotnet", "CourseSchedule.Api.dll"]
```

Với Student Service, đổi DLL:

```dockerfile
ENTRYPOINT ["dotnet", "StudentAttendance.Api.dll"]
```

Với Payment Service, đổi DLL:

```dockerfile
ENTRYPOINT ["dotnet", "PaymentReport.Api.dll"]
```

Với Gateway, đổi DLL:

```dockerfile
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
```

Nếu dự án dùng .NET 10 thì đổi image:

```dockerfile
mcr.microsoft.com/dotnet/sdk:10.0
mcr.microsoft.com/dotnet/aspnet:10.0
```

---

## 5. Dockerfile khi solution nằm nhiều project

Nếu service có cấu trúc:

```txt
CourseSchedule.Api/
CourseSchedule.Application/
CourseSchedule.Domain/
CourseSchedule.Infrastructure/
CourseSchedule.sln
```

Dockerfile có thể dùng dạng:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore CourseSchedule.sln
RUN dotnet publish CourseSchedule.Api/CourseSchedule.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

ENTRYPOINT ["dotnet", "CourseSchedule.Api.dll"]
```

Tương tự cho service khác:

```txt
StudentAttendance.Api/StudentAttendance.Api.csproj
PaymentReport.Api/PaymentReport.Api.csproj
ApiGateway/ApiGateway.csproj
```

---

## 6. Tạo Web Service trên Render

Với từng service:

1. Vào Render Dashboard.
2. Chọn **New**.
3. Chọn **Web Service**.
4. Connect GitHub repository.
5. Chọn repo `training-center-management`.
6. Chọn branch deploy, thường là `main` hoặc `develop`.
7. Chọn Runtime là **Docker**.
8. Cấu hình Root Directory.
9. Cấu hình Dockerfile Path nếu cần.
10. Thêm environment variables.
11. Deploy.

---

## 7. Cấu hình Render cho từng service

### 7.1 Course & Schedule Service

| Mục | Giá trị |
|---|---|
| Name | `course-schedule-service` |
| Root Directory | `services/course-schedule-service` |
| Runtime | Docker |
| Dockerfile Path | `services/course-schedule-service/Dockerfile` |
| Health Check Path | `/swagger` hoặc `/health` |

Env cần có:

```txt
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=<Azure SQL CourseScheduleDb>
Jwt__Issuer=TrainingCenter
Jwt__Audience=TrainingCenterUsers
Jwt__SecretKey=<same-secret>
Cors__AllowedOrigins__0=https://training-center.vercel.app
Cors__AllowedOrigins__1=http://localhost:5173
ExternalServices__PaymentReportBaseUrl=https://payment-report-service.onrender.com
```

---

### 7.2 Student & Attendance Service

| Mục | Giá trị |
|---|---|
| Name | `student-attendance-service` |
| Root Directory | `services/student-attendance-service` |
| Runtime | Docker |
| Dockerfile Path | `services/student-attendance-service/Dockerfile` |
| Health Check Path | `/swagger` hoặc `/health` |

Env cần có:

```txt
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=<Azure SQL StudentAttendanceDb>
Jwt__Issuer=TrainingCenter
Jwt__Audience=TrainingCenterUsers
Jwt__SecretKey=<same-secret>
Cors__AllowedOrigins__0=https://training-center.vercel.app
Cors__AllowedOrigins__1=http://localhost:5173
ExternalServices__CourseScheduleBaseUrl=https://course-schedule-service.onrender.com
ExternalServices__PaymentReportBaseUrl=https://payment-report-service.onrender.com
```

---

### 7.3 Payment & Report/Auth Service

| Mục | Giá trị |
|---|---|
| Name | `payment-report-service` |
| Root Directory | `services/payment-report-service` |
| Runtime | Docker |
| Dockerfile Path | `services/payment-report-service/Dockerfile` |
| Health Check Path | `/swagger` hoặc `/health` |

Env cần có:

```txt
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=<Azure SQL PaymentReportDb>
Jwt__Issuer=TrainingCenter
Jwt__Audience=TrainingCenterUsers
Jwt__SecretKey=<same-secret>
Cors__AllowedOrigins__0=https://training-center.vercel.app
Cors__AllowedOrigins__1=http://localhost:5173
ExternalServices__CourseScheduleBaseUrl=https://course-schedule-service.onrender.com
ExternalServices__StudentAttendanceBaseUrl=https://student-attendance-service.onrender.com
```

---

### 7.4 API Gateway

| Mục | Giá trị |
|---|---|
| Name | `training-center-gateway` |
| Root Directory | `gateway/api-gateway` |
| Runtime | Docker |
| Dockerfile Path | `gateway/api-gateway/Dockerfile` |
| Health Check Path | `/health` nếu có |

Env cần có:

```txt
ASPNETCORE_ENVIRONMENT=Production
Jwt__Issuer=TrainingCenter
Jwt__Audience=TrainingCenterUsers
Jwt__SecretKey=<same-secret>
Cors__AllowedOrigins__0=https://training-center.vercel.app
Cors__AllowedOrigins__1=http://localhost:5173
ServiceUrls__CourseSchedule=https://course-schedule-service.onrender.com
ServiceUrls__StudentAttendance=https://student-attendance-service.onrender.com
ServiceUrls__PaymentReport=https://payment-report-service.onrender.com
```

---

## 8. Ocelot Gateway configuration

File chính:

```txt
gateway/api-gateway/ApiGateway/ocelot.json
```

Ví dụ cấu hình route:

```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/course-schedule/{everything}",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [
        { "Host": "course-schedule-service.onrender.com", "Port": 443 }
      ],
      "UpstreamPathTemplate": "/api/course-schedule/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "PATCH", "DELETE" ]
    },
    {
      "DownstreamPathTemplate": "/api/student-attendance/{everything}",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [
        { "Host": "student-attendance-service.onrender.com", "Port": 443 }
      ],
      "UpstreamPathTemplate": "/api/student-attendance/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "PATCH", "DELETE" ]
    },
    {
      "DownstreamPathTemplate": "/api/payment-report/{everything}",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [
        { "Host": "payment-report-service.onrender.com", "Port": 443 }
      ],
      "UpstreamPathTemplate": "/api/payment-report/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "PATCH", "DELETE" ]
    }
  ],
  "GlobalConfiguration": {
    "BaseUrl": "https://training-center-gateway.onrender.com"
  }
}
```

---

## 9. Bật Swagger khi deploy

Trong demo/staging, nên bật Swagger để kiểm thử:

```txt
https://course-schedule-service.onrender.com/swagger
https://student-attendance-service.onrender.com/swagger
https://payment-report-service.onrender.com/swagger
```

Production thật có thể tắt Swagger hoặc chỉ bảo vệ bằng auth.

---

## 10. Health check

Khuyến nghị mỗi backend có endpoint:

```http
GET /health
```

Response:

```json
{ "status": "Healthy" }
```

Render có thể dùng `/health` làm Health Check Path.

Nếu chưa có `/health`, tạm dùng `/swagger`.

---

## 11. Kiểm tra sau khi deploy

Kiểm tra từng service:

```txt
https://course-schedule-service.onrender.com/swagger
https://student-attendance-service.onrender.com/swagger
https://payment-report-service.onrender.com/swagger
```

Kiểm tra gateway route:

```txt
https://training-center-gateway.onrender.com/api/course-schedule/health
https://training-center-gateway.onrender.com/api/student-attendance/health
https://training-center-gateway.onrender.com/api/payment-report/health
```

Kiểm tra login:

```http
POST https://training-center-gateway.onrender.com/api/payment-report/auth/login
```

---

## 12. Lỗi thường gặp

### App deploy thành công nhưng không truy cập được

Kiểm tra:

```txt
ASPNETCORE_URLS=http://0.0.0.0:${PORT}
```

App phải listen đúng port Render cấp.

### Lỗi kết nối database

Kiểm tra:

```txt
ConnectionStrings__DefaultConnection
Azure SQL firewall
Username/password
Database name
Encrypt=True
```

### Lỗi 401/403

Kiểm tra:

```txt
Jwt__Issuer
Jwt__Audience
Jwt__SecretKey
Role claim
Authorization header
```

### Lỗi CORS

Kiểm tra:

```txt
Cors__AllowedOrigins__0=https://training-center.vercel.app
Frontend đang gọi đúng Gateway URL chưa
Gateway có bật CORS chưa
```

### Gateway không route được

Kiểm tra:

```txt
DownstreamHostAndPorts
DownstreamScheme
UpstreamPathTemplate
Service URL trên Render có mở được không
```
