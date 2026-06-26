namespace PaymentReport.Api.Infrastructure;

public enum Role
{
    Admin,
    Teacher,
    Student,
    InternalService
}

public enum UserStatus
{
    Active,
    Locked
}

public enum InvoiceStatus
{
    Unpaid,
    Partial,
    Paid,
    Overdue,
    Cancelled
}

public class UserAccount
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string FullName { get; set; } = string.Empty;
    public Role Role { get; set; }
    public int RelatedEntityId { get; set; }
    public string RelatedEntityType { get; set; } = string.Empty;
    public bool MustChangePassword { get; set; }
    public UserStatus Status { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class TuitionFee
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public int? ClassId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime EffectiveFrom { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Invoice
{
    public int Id { get; set; }
    public string InvoiceCode { get; set; } = string.Empty;
    public int StudentId { get; set; }
    public int EnrollmentId { get; set; }
    public int CourseId { get; set; }
    public int ClassId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal DebtAmount { get; set; }
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public string CreatedSource { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class PaymentTransaction
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime PaidAt { get; set; }
    public string CollectedBy { get; set; } = string.Empty;
    public string? Note { get; set; }
}

public class RefreshToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool Revoked { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DebtStatus
{
    public int StudentId { get; set; }
    public decimal TotalDebt { get; set; }
    public int OverdueCount { get; set; }
    public bool HasOverdueDebt { get; set; }
}
