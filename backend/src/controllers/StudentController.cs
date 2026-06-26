using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentReport.Api.Infrastructure;
using PaymentReport.Api.Services;

namespace PaymentReport.Api.Controllers;

[ApiController]
[Route("api/payment-report/student")]
[Authorize(Roles = "Student")]
public class StudentController : ControllerBase
{
    private readonly InvoiceService _invoiceService;
    private readonly PaymentService _paymentService;

    public StudentController(InvoiceService invoiceService, PaymentService paymentService)
    {
        _invoiceService = invoiceService;
        _paymentService = paymentService;
    }

    private int? GetCurrentStudentId()
    {
        var relatedEntityIdClaim = User.FindFirstValue("relatedEntityId");
        return int.TryParse(relatedEntityIdClaim, out var relatedEntityId) ? relatedEntityId : null;
    }

    [HttpGet("dashboard-summary")]
    public IActionResult DashboardSummary()
    {
        var studentId = GetCurrentStudentId();
        if (studentId == null) return Unauthorized();

        var invoices = _invoiceService.GetInvoicesByStudentId(studentId.Value).ToList();
        var totalDebt = invoices.Sum(i => i.DebtAmount);
        var totalPaid = invoices.Sum(i => i.PaidAmount);
        var overdueCount = invoices.Count(i => i.Status == InvoiceStatus.Overdue);
        var nextDueInvoice = invoices
            .Where(i => i.Status != InvoiceStatus.Paid)
            .OrderBy(i => i.DueDate)
            .Select(i => new { i.Id, i.InvoiceCode, i.DueDate, i.DebtAmount, status = i.Status.ToString() })
            .FirstOrDefault();

        return Ok(new
        {
            totalDebt,
            totalPaid,
            invoiceCount = invoices.Count,
            overdueCount,
            hasOverdueDebt = overdueCount > 0,
            nextDueInvoice
        });
    }

    [HttpGet("invoices")]
    public IActionResult GetInvoices()
    {
        var studentId = GetCurrentStudentId();
        if (studentId == null) return Unauthorized();

        var invoices = _invoiceService.GetInvoicesByStudentId(studentId.Value).ToList();
        var result = invoices.Select(invoice => new
        {
            invoice.Id,
            invoice.InvoiceCode,
            invoice.StudentId,
            invoice.EnrollmentId,
            invoice.CourseId,
            invoice.ClassId,
            invoice.TotalAmount,
            invoice.DiscountAmount,
            invoice.FinalAmount,
            invoice.PaidAmount,
            invoice.DebtAmount,
            invoice.DueDate,
            status = invoice.Status.ToString(),
            invoice.CreatedSource,
            invoice.CreatedAt
        }).ToList();
        return Ok(result);
    }

    [HttpGet("invoices/{invoiceId}")]
    public IActionResult GetInvoice(int invoiceId)
    {
        var studentId = GetCurrentStudentId();
        if (studentId == null) return Unauthorized();

        var invoice = _invoiceService.GetInvoiceById(invoiceId);
        if (invoice == null || invoice.StudentId != studentId.Value) return NotFound();
        
        var result = new
        {
            invoice.Id,
            invoice.InvoiceCode,
            invoice.StudentId,
            invoice.EnrollmentId,
            invoice.CourseId,
            invoice.ClassId,
            invoice.TotalAmount,
            invoice.DiscountAmount,
            invoice.FinalAmount,
            invoice.PaidAmount,
            invoice.DebtAmount,
            invoice.DueDate,
            status = invoice.Status.ToString(),
            invoice.CreatedSource,
            invoice.CreatedAt
        };
        return Ok(result);
    }

    [HttpGet("invoices/overdue")]
    public IActionResult GetOverdueInvoices()
    {
        var studentId = GetCurrentStudentId();
        if (studentId == null) return Unauthorized();

        var invoices = _invoiceService.GetInvoicesByStudentId(studentId.Value)
            .Where(i => i.Status.ToString() == "Overdue").ToList();
        var result = invoices.Select(invoice => new
        {
            invoice.Id,
            invoice.InvoiceCode,
            invoice.StudentId,
            invoice.EnrollmentId,
            invoice.CourseId,
            invoice.ClassId,
            invoice.TotalAmount,
            invoice.DiscountAmount,
            invoice.FinalAmount,
            invoice.PaidAmount,
            invoice.DebtAmount,
            invoice.DueDate,
            status = invoice.Status.ToString(),
            invoice.CreatedSource,
            invoice.CreatedAt
        }).ToList();
        return Ok(result);
    }

    [HttpGet("payments")]
    public IActionResult GetPayments()
    {
        var studentId = GetCurrentStudentId();
        if (studentId == null) return Unauthorized();

        var payments = _paymentService.GetPaymentsByStudent(studentId.Value)
            .Select(payment => new
            {
                payment.Id,
                payment.InvoiceId,
                payment.Amount,
                payment.PaymentMethod,
                payment.CollectedBy,
                payment.PaidAt,
                payment.Note,
                invoiceCode = _invoiceService.GetInvoiceById(payment.InvoiceId)?.InvoiceCode,
                invoiceDueDate = _invoiceService.GetInvoiceById(payment.InvoiceId)?.DueDate
            });

        return Ok(payments);
    }

    [HttpGet("debt")]
    public IActionResult GetDebt()
    {
        var studentId = GetCurrentStudentId();
        if (studentId == null) return Unauthorized();

        var invoices = _invoiceService.GetInvoicesByStudentId(studentId.Value).ToList();
        var totalDebt = invoices.Sum(i => i.DebtAmount);
        var overdueCount = invoices.Count(i => i.Status == InvoiceStatus.Overdue);
        var overdueInvoices = invoices.Where(i => i.Status == InvoiceStatus.Overdue)
            .Select(i => new { i.Id, i.InvoiceCode, i.DebtAmount, i.DueDate })
            .ToList();

        return Ok(new
        {
            studentId,
            totalDebt,
            overdueCount,
            hasOverdueDebt = overdueCount > 0,
            overdueInvoices
        });
    }

    [HttpGet("receipts/{paymentId}")]
    public IActionResult GetReceipt(int paymentId)
    {
        var studentId = GetCurrentStudentId();
        if (studentId == null) return Unauthorized();

        var payment = _paymentService.GetPaymentById(paymentId);
        if (payment == null) return NotFound();

        var invoice = _invoiceService.GetInvoiceById(payment.InvoiceId);
        if (invoice == null || invoice.StudentId != studentId.Value) return NotFound();

        return Ok(new { payment, invoice });
    }
}
