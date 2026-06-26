using Microsoft.EntityFrameworkCore;
using PaymentReport.Api.Infrastructure;

namespace PaymentReport.Api.Services;

public class PaymentService
{
    private readonly AppDbContext _db;
    private readonly InvoiceService _invoiceService;

    public PaymentService(AppDbContext db, InvoiceService invoiceService)
    {
        _db = db;
        _invoiceService = invoiceService;
    }

    public PaymentTransaction? CreatePayment(PaymentTransaction payment)
    {
        var invoice = _invoiceService.GetInvoiceById(payment.InvoiceId);
        if (invoice == null) return null;

        payment.PaidAt = DateTime.UtcNow;
        _db.Payments.Add(payment);
        _db.SaveChanges();
        _invoiceService.ApplyPayment(payment.InvoiceId, payment.Amount);
        return payment;
    }

    public IEnumerable<PaymentTransaction> GetPaymentsByInvoice(int invoiceId) => _db.Payments.Where(p => p.InvoiceId == invoiceId).ToList();

    public PaymentTransaction? GetPaymentById(int paymentId) => _db.Payments.Find(paymentId);

    public IEnumerable<PaymentTransaction> GetPaymentsByStudent(int studentId)
    {
        var invoiceIds = _db.Invoices.Where(i => i.StudentId == studentId).Select(i => i.Id).ToList();
        return _db.Payments.Where(p => invoiceIds.Contains(p.InvoiceId)).ToList();
    }
}
