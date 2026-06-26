# Service 3 - Payment & Report

## Mục tiêu

Payment & Report Service là service thứ 3 trong hệ thống và chịu trách nhiệm toàn bộ nghiệp vụ:

- Xác thực / đăng nhập / JWT
- Quản lý tài khoản người dùng (Admin / Teacher / Student)
- Đổi mật khẩu lần đầu
- Quản lý học phí và cấu hình tuition fees
- Quản lý invoice và công nợ
- Hỗ trợ thanh toán và ghi nhận payment
- Tạo báo cáo doanh thu, công nợ, báo cáo lớp

## Chức năng đã đạt được

- Đăng nhập bằng JWT với role: `Admin`, `Teacher`, `Student`, `InternalService`
- Seed dữ liệu mẫu cho `admin`, `student`, `teacher`
- Quản lý tài khoản người dùng và đổi mật khẩu
- Tạo / xem / cập nhật tuition fee
- Tạo và theo dõi invoice, tính toán `PaidAmount`, `DebtAmount`, `Status`
- Ghi nhận giao dịch thanh toán và liên kết với invoice
- Hiển thị báo cáo lớp, tổng nợ và số hóa đơn quá hạn cho teacher
- API internal để tạo tài khoản teacher/student từ hệ thống ngoại vi
- Flow phân quyền controller theo role với `Authorize(Roles = "...")`

Service này phải liên kết được với:

- N1 - Course & Schedule Service: tạo tài khoản giáo viên, truy vấn thông tin lớp/khóa học, báo cáo lớp
- N2 - Student & Attendance Service: tạo tài khoản học viên, tạo invoice sau khi Enrollment duyệt, kiểm tra công nợ học viên

## Cấu trúc thư mục

- `README.md` - mô tả mục tiêu và tích hợp
- `openapi.yaml` - tài liệu API chính cho Service 3

## Kết nối với N1 và N2

### 1. Tạo tài khoản giáo viên từ N1

Course & Schedule Service (N1) tạo profile giáo viên và gọi internal API của Payment & Report Service để tạo `UserAccount` với role `Teacher`.

- Endpoint: `POST /api/payment-report/internal/users/create-teacher-account`
- Trả về: `userId`, `username`, `temporaryPassword`

### 2. Tạo tài khoản học viên từ N2

Student & Attendance Service (N2) tạo profile học viên và gọi internal API của Payment & Report Service để tạo `UserAccount` với role `Student`.

- Endpoint: `POST /api/payment-report/internal/users/create-student-account`
- Trả về: `userId`, `username`, `temporaryPassword`

### 3. Tạo invoice sau khi N2 duyệt Enrollment

Khi Student & Attendance Service (N2) duyệt đăng ký thành công, nó gọi Payment & Report Service để:

- kiểm tra học phí theo `courseId` / `classId`
- tạo `Invoice`
- trả về `invoiceId` cho N2 để liên kết với `Enrollment`

### 4. Tra cứu công nợ và báo cáo cho N1/N2

Payment & Report Service cung cấp internal API để N1/N2 dùng khi cần kiểm tra công nợ học viên hoặc lấy thông tin user.

- `GET /api/payment-report/internal/users/{userId}`
- `GET /api/payment-report/internal/students/{studentId}/debt-status`

## Quy ước JWT và role

Service 3 là nơi phát hành JWT. N1 và N2 chỉ validate token bằng cùng cấu hình sau:

- `role` claim: `Admin`, `Teacher`, `Student`, `InternalService`
- `sub` hoặc `userId` claim: id người dùng
- `mustChangePassword` claim: kiểm tra flow đổi mật khẩu lần đầu

## Gợi ý triển khai

- Nếu backend dùng .NET, Service 3 có thể là một Web API project với các controller `AuthController`, `UsersController`, `TuitionFeesController`, `InvoicesController`, `PaymentsController`, `ReportsController`, `InternalController`.
- Service 3 cần database riêng: `PaymentReportDb`.
- Service 3 không đọc dữ liệu N1/N2 trực tiếp; chỉ giao tiếp qua API.
- Service 3 nên có API Gateway hoặc chia rõ prefix `/api/payment-report`.
