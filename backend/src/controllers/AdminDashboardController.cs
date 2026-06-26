using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentReport.Api.Infrastructure;
using PaymentReport.Api.Services;

namespace PaymentReport.Api.Controllers;

[ApiController]
[Route("api/payment-report/admin/dashboard")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly InvoiceService _invoiceService;
    private readonly PaymentService _paymentService;
    private readonly UserService _userService;
    private readonly AppDbContext _dbContext;

    public AdminDashboardController(InvoiceService invoiceService, PaymentService paymentService, UserService userService, AppDbContext dbContext)
    {
        _invoiceService = invoiceService;
        _paymentService = paymentService;
        _userService = userService;
        _dbContext = dbContext;
    }

    // Dashboard summary endpoints
    [HttpGet("/api/payment-report/admin/dashboard-summary")]
    public IActionResult GetSummary()
    {
        var invoices = _invoiceService.GetInvoices().ToList();
        var totalRevenue = invoices.Where(i => i.Status.ToString() == "Paid").Sum(i => i.FinalAmount);
        var totalDebt = invoices.Sum(i => i.DebtAmount);
        var overdueCount = invoices.Count(i => i.Status.ToString() == "Overdue");
        var invoiceCount = invoices.Count;

        return Ok(new { totalRevenue, totalDebt, overdueCount, invoiceCount });
    }

    [HttpGet("/api/payment-report/admin/reports/revenue")]
    public IActionResult GetRevenueReport()
    {
        var invoices = _invoiceService.GetInvoices().ToList();
        var paidInvoices = invoices.Where(i => i.Status.ToString() == "Paid").ToList();
        var totalRevenue = paidInvoices.Sum(i => i.FinalAmount);
        var monthlyRevenue = paidInvoices.GroupBy(i => i.CreatedAt.Month)
            .Select(g => new { month = g.Key, revenue = g.Sum(i => i.FinalAmount) });

        return Ok(new { totalRevenue, monthlyRevenue });
    }

    [HttpGet("/api/payment-report/admin/reports/debt")]
    public IActionResult GetDebtReport()
    {
        var invoices = _invoiceService.GetInvoices().ToList();
        var totalDebt = invoices.Sum(i => i.DebtAmount);
        var studentDebts = invoices.GroupBy(i => i.StudentId)
            .Select(g => new { studentId = g.Key, totalDebt = g.Sum(i => i.DebtAmount), invoiceCount = g.Count() })
            .OrderByDescending(s => s.totalDebt);

        return Ok(new { totalDebt, studentDebts });
    }

    [HttpGet("/api/payment-report/admin/reports/classes/{classId}")]
    public IActionResult GetClassReport(int classId)
    {
        var invoices = _invoiceService.GetInvoicesByClassId(classId).ToList();
        var studentInvoices = invoices.GroupBy(i => i.StudentId)
            .Select(g => new { studentId = g.Key, totalDebt = g.Sum(i => i.DebtAmount), status = g.First().Status.ToString() });

        return Ok(new { classId, invoiceCount = invoices.Count, totalDebt = invoices.Sum(i => i.DebtAmount), studentInvoices });
    }

    [HttpGet("/api/payment-report/admin/students/{studentId}/debt")]
    public IActionResult GetStudentDebt(int studentId)
    {
        var invoices = _invoiceService.GetInvoicesByStudentId(studentId).ToList();
        if (!invoices.Any()) return NotFound(new { message = "Student debt not found" });

        var totalDebt = invoices.Sum(i => i.DebtAmount);
        var unpaidInvoices = invoices.Select(i => new
        {
            i.Id,
            i.InvoiceCode,
            i.TotalAmount,
            i.PaidAmount,
            i.DebtAmount,
            dueDate = i.DueDate,
            status = i.Status.ToString()
        }).ToList();

        return Ok(new { studentId, totalDebt, unpaidInvoices });
    }

    [HttpGet("/api/payment-report/admin/debts")]
    public IActionResult GetDebts()
    {
        var invoices = _invoiceService.GetInvoices().Where(i => i.DebtAmount > 0).ToList();
        var debts = invoices.GroupBy(i => i.StudentId)
            .Select(g => new
            {
                studentId = g.Key,
                totalDebt = g.Sum(i => i.DebtAmount),
                invoiceCount = g.Count(),
                overdueCount = g.Count(i => i.Status.ToString() == "Overdue")
            })
            .OrderByDescending(d => d.totalDebt);

        return Ok(debts);
    }

    // Invoice management endpoints
    [HttpGet("invoices")]
    public IActionResult GetInvoices()
    {
        var invoices = _invoiceService.GetInvoices().ToList();
        var result = invoices.Select(invoice => new
        {
            id = invoice.Id,
            invoiceCode = invoice.InvoiceCode,
            studentId = invoice.StudentId,
            studentName = _dbContext.Users.FirstOrDefault(u => u.Id == invoice.StudentId)?.FullName ?? "Unknown",
            totalAmount = invoice.TotalAmount,
            discountAmount = invoice.DiscountAmount,
            finalAmount = invoice.FinalAmount,
            paidAmount = invoice.PaidAmount,
            debtAmount = invoice.DebtAmount,
            status = invoice.Status.ToString(),
            dueDate = invoice.DueDate,
            createdAt = invoice.CreatedAt
        }).ToList();

        return Ok(result);
    }

    // Payment management endpoints  
    [HttpGet("payments")]
    public IActionResult GetPayments()
    {
        var payments = _dbContext.Payments.ToList();
        var result = payments.Select(payment => new
        {
            id = payment.Id,
            invoiceCode = _dbContext.Invoices.FirstOrDefault(i => i.Id == payment.InvoiceId)?.InvoiceCode ?? "N/A",
            invoiceId = payment.InvoiceId,
            amount = payment.Amount,
            paymentMethod = payment.PaymentMethod,
            collectedBy = payment.CollectedBy,
            paidAt = payment.PaidAt,
            note = payment.Note,
            createdAt = payment.PaidAt
        }).ToList();

        return Ok(result);
    }

    [HttpPost("payments")]
    public IActionResult CreatePayment([FromBody] CreatePaymentRequest request)
    {
        if (request.InvoiceId == null)
            return BadRequest(new { message = "InvoiceId is required" });

        var invoice = _invoiceService.GetInvoiceById(request.InvoiceId.Value);
        if (invoice == null)
            return NotFound(new { message = "Invoice not found" });

        var payment = new PaymentTransaction
        {
            InvoiceId = request.InvoiceId.Value,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            CollectedBy = request.CollectedBy,
            Note = request.Note,
            PaidAt = DateTime.UtcNow
        };

        var created = _paymentService.CreatePayment(payment);
        return Created(string.Empty, created);
    }
}
