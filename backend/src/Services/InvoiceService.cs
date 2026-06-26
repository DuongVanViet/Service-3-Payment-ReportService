using PaymentReport.Api.Infrastructure;

namespace PaymentReport.Api.Services;

public class InvoiceService
{
    private readonly AppDbContext _db;

    public InvoiceService(AppDbContext db)
    {
        _db = db;
    }

    public IEnumerable<Invoice> GetInvoices() => _db.Invoices.ToList();

    public Invoice? GetInvoiceById(int invoiceId) => _db.Invoices.Find(invoiceId);

    public Invoice? GetInvoiceByEnrollment(int enrollmentId) => _db.Invoices.FirstOrDefault(i => i.EnrollmentId == enrollmentId);

    public IEnumerable<Invoice> GetInvoicesByStudentId(int studentId) => _db.Invoices.Where(i => i.StudentId == studentId).ToList();

    public IEnumerable<Invoice> GetInvoicesByClassId(int classId) => _db.Invoices.Where(i => i.ClassId == classId).ToList();

    public IEnumerable<Invoice> GetOverdueInvoices() => _db.Invoices.Where(i => i.Status == InvoiceStatus.Overdue).ToList();

    public Invoice CreateInvoice(Invoice invoice)
    {
        invoice.InvoiceCode = $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}";
        invoice.PaidAmount = 0;
        invoice.DebtAmount = invoice.FinalAmount;
        invoice.Status = InvoiceStatus.Unpaid;
        invoice.CreatedAt = DateTime.UtcNow;
        _db.Invoices.Add(invoice);
        _db.SaveChanges();
        return invoice;
    }

    public Invoice? CreateInvoiceFromEnrollment(int studentId, int enrollmentId, int courseId, int classId, decimal totalAmount, decimal discountAmount, DateTime dueDate)
    {
        if (GetInvoiceByEnrollment(enrollmentId) != null) return null;
        var invoice = new Invoice
        {
            StudentId = studentId,
            EnrollmentId = enrollmentId,
            CourseId = courseId,
            ClassId = classId,
            TotalAmount = totalAmount,
            DiscountAmount = discountAmount,
            FinalAmount = totalAmount - discountAmount,
            DueDate = dueDate,
            CreatedSource = "EnrollmentApproval"
        };
        return CreateInvoice(invoice);
    }

    public IEnumerable<Invoice> GenerateInvoicesByClass(int classId, IEnumerable<int> studentIds, int courseId, decimal totalAmount, decimal discountAmount, DateTime dueDate)
    {
        var invoices = new List<Invoice>();
        foreach (var studentId in studentIds)
        {
            var invoice = new Invoice
            {
                StudentId = studentId,
                EnrollmentId = 0,
                CourseId = courseId,
                ClassId = classId,
                TotalAmount = totalAmount,
                DiscountAmount = discountAmount,
                FinalAmount = totalAmount - discountAmount,
                DueDate = dueDate,
                CreatedSource = "BulkGenerate"
            };
            invoices.Add(CreateInvoice(invoice));
        }
        return invoices;
    }

    public bool CancelInvoice(int invoiceId)
    {
        var invoice = GetInvoiceById(invoiceId);
        if (invoice == null) return false;
        invoice.Status = InvoiceStatus.Cancelled;
        _db.SaveChanges();
        return true;
    }

    public bool ApplyPayment(int invoiceId, decimal amount)
    {
        var invoice = GetInvoiceById(invoiceId);
        if (invoice == null) return false;
        invoice.PaidAmount += amount;
        invoice.DebtAmount = Math.Max(0, invoice.FinalAmount - invoice.PaidAmount);
        invoice.Status = invoice.DebtAmount == 0 ? InvoiceStatus.Paid : InvoiceStatus.Partial;
        _db.SaveChanges();
        return true;
    }
}
