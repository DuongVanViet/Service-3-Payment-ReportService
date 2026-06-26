namespace PaymentReport.Api.Infrastructure;

public class InMemoryDatabase
{
    public List<UserAccount> Users { get; } = new();
    public List<TuitionFee> TuitionFees { get; } = new();
    public List<Invoice> Invoices { get; } = new();
    public List<PaymentTransaction> Payments { get; } = new();

    private int _nextUserId = 1;
    private int _nextTuitionFeeId = 1;
    private int _nextInvoiceId = 1;
    private int _nextPaymentId = 1;

    public InMemoryDatabase()
    {
        Users.Add(new UserAccount
        {
            Id = _nextUserId++,
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
        });

        Users.Add(new UserAccount
        {
            Id = _nextUserId++,
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
        });

        TuitionFees.Add(new TuitionFee
        {
            Id = _nextTuitionFeeId++,
            CourseId = 10,
            ClassId = 100,
            Amount = 5000000,
            Currency = "VND",
            EffectiveFrom = new DateTime(2026, 1, 1),
            IsActive = true
        });

        TuitionFees.Add(new TuitionFee
        {
            Id = _nextTuitionFeeId++,
            CourseId = 10,
            ClassId = null,
            Amount = 5200000,
            Currency = "VND",
            EffectiveFrom = new DateTime(2026, 6, 1),
            IsActive = true
        });

        Invoices.Add(new Invoice
        {
            Id = _nextInvoiceId++,
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
    }

    public int GetNextUserId() => _nextUserId++;
    public int GetNextTuitionFeeId() => _nextTuitionFeeId++;
    public int GetNextInvoiceId() => _nextInvoiceId++;
    public int GetNextPaymentId() => _nextPaymentId++;
}
