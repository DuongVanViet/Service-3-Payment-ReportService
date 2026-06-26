using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PaymentReport.Api.Infrastructure;
using PaymentReport.Api.Services;
using System.Text;
 
var builder = WebApplication.CreateBuilder(args);
 
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
 
// Chỉ giữ 1 policy CORS duy nhất, liệt kê đủ các origin cần thiết
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:5174",
                "http://localhost:5175",
                "http://localhost:5176",
                "http://127.0.0.1:5173",
                "http://127.0.0.1:5174",
                "http://127.0.0.1:5175",
                "http://127.0.0.1:5176"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
 
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
 
var jwtSecret = builder.Configuration.GetValue<string>("JwtSecret") ?? "super-secret-payment-report-service";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
 
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = signingKey,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30)
    };
});
 
builder.Services.AddAuthorization();
 
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TuitionFeeService>();
builder.Services.AddScoped<InvoiceService>();
builder.Services.AddScoped<PaymentService>();
 
var app = builder.Build();
 
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
 
    // Drop and recreate database to ensure all tables exist
    db.Database.EnsureDeleted();
    db.Database.EnsureCreated();
 
    // Seed sample data
    db.Users.AddRange(new[]
    {
        new UserAccount
        {
            Username = "admin@training.local",
            Email = "admin@training.local",
            FullName = "System Admin",
            Phone = "0900000000",
            Role = Role.Admin,
            RelatedEntityId = 0,
            RelatedEntityType = "Admin",
            MustChangePassword = false,
            Status = UserStatus.Active,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        },
        new UserAccount
        {
            Username = "student1@training.local",
            Email = "student1@training.local",
            FullName = "Student One",
            Phone = "0910000001",
            Role = Role.Student,
            RelatedEntityId = 101,
            RelatedEntityType = "Student",
            MustChangePassword = false,
            Status = UserStatus.Active,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        },
        new UserAccount
        {
            Username = "teacher1@training.local",
            Email = "teacher1@training.local",
            FullName = "Teacher One",
            Phone = "0920000002",
            Role = Role.Teacher,
            RelatedEntityId = 201,
            RelatedEntityType = "Teacher",
            MustChangePassword = false,
            Status = UserStatus.Active,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Teacher@123"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }
    });
 
    db.TuitionFees.AddRange(new[]
    {
        new TuitionFee
        {
            CourseId = 10,
            ClassId = 100,
            Amount = 5000000,
            Currency = "VND",
            EffectiveFrom = new DateTime(2026, 1, 1),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        },
        new TuitionFee
        {
            CourseId = 10,
            ClassId = null,
            Amount = 5200000,
            Currency = "VND",
            EffectiveFrom = new DateTime(2026, 6, 1),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        }
    });
 
    db.Invoices.Add(new Invoice
    {
        InvoiceCode = "INV-2026-0001",
        StudentId = 101,
        EnrollmentId = 5001,
        CourseId = 10,
        ClassId = 100,
        TotalAmount = 5000000,
        DiscountAmount = 0,
        FinalAmount = 5000000,
        PaidAmount = 0,
        DebtAmount = 5000000,
        DueDate = new DateTime(2026, 7, 1),
        Status = InvoiceStatus.Unpaid,
        CreatedAt = DateTime.UtcNow
    });
 
    db.SaveChanges();
}
 
app.UseSwagger();
app.UseSwaggerUI();
 
// Dùng đúng tên policy đã đăng ký ở trên
app.UseCors("AllowFrontend");
 
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
 