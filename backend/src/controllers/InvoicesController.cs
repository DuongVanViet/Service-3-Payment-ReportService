using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentReport.Api.Infrastructure;
using PaymentReport.Api.Services;

namespace PaymentReport.Api.Controllers;

[ApiController]
[Route("api/payment-report/admin/invoices")]
[Authorize(Roles = "Admin")]
public class InvoicesController : ControllerBase
{
    private readonly InvoiceService _invoiceService;
    private readonly AppDbContext _dbContext;

    public InvoicesController(InvoiceService invoiceService, AppDbContext dbContext)
    {
        _invoiceService = invoiceService;
        _dbContext = dbContext;
    }

    [HttpGet]
    public IActionResult GetAll(
        [FromQuery] int? studentId,
        [FromQuery] int? classId,
        [FromQuery] string? status,
        [FromQuery] DateTime? fromCreatedAt,
        [FromQuery] DateTime? toCreatedAt,
        [FromQuery] bool? overdue)
    {
        var invoices = _invoiceService.GetInvoices().AsQueryable();

        if (studentId.HasValue)
            invoices = invoices.Where(i => i.StudentId == studentId.Value);

        if (classId.HasValue)
            invoices = invoices.Where(i => i.ClassId == classId.Value);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<InvoiceStatus>(status, true, out var parsedStatus))
            invoices = invoices.Where(i => i.Status == parsedStatus);

        if (fromCreatedAt.HasValue)
            invoices = invoices.Where(i => i.CreatedAt >= fromCreatedAt.Value);

        if (toCreatedAt.HasValue)
            invoices = invoices.Where(i => i.CreatedAt <= toCreatedAt.Value);

        if (overdue.HasValue)
            invoices = overdue.Value ? invoices.Where(i => i.Status == InvoiceStatus.Overdue) : invoices.Where(i => i.Status != InvoiceStatus.Overdue);

        var result = invoices.ToList().Select(invoice => new
        {
            invoice.Id,
            invoice.InvoiceCode,
            invoice.StudentId,
            studentName = _dbContext.Users.FirstOrDefault(u => u.RelatedEntityId == invoice.StudentId)?.FullName ?? "Unknown",
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

    [HttpGet("overdue")]
    public IActionResult GetOverdue()
    {
        var invoices = _invoiceService.GetOverdueInvoices().ToList();
        var result = invoices.Select(invoice => new
        {
            invoice.Id,
            invoice.InvoiceCode,
            invoice.StudentId,
            studentName = _dbContext.Users.FirstOrDefault(u => u.RelatedEntityId == invoice.StudentId)?.FullName ?? "Unknown",
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

    [HttpGet("{invoiceId}")]
    public IActionResult GetById(int invoiceId)
    {
        var invoice = _invoiceService.GetInvoiceById(invoiceId);
        if (invoice == null) return NotFound();
        var result = new
        {
            invoice.Id,
            invoice.InvoiceCode,
            invoice.StudentId,
            studentName = _dbContext.Users.FirstOrDefault(u => u.RelatedEntityId == invoice.StudentId)?.FullName ?? "Unknown",
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

    [HttpPost]
    public IActionResult Create([FromBody] CreateInvoiceRequest request)
    {
        var invoice = new Invoice
        {
            StudentId = request.StudentId,
            EnrollmentId = request.EnrollmentId,
            CourseId = request.CourseId,
            ClassId = request.ClassId,
            TotalAmount = request.TotalAmount,
            DiscountAmount = request.DiscountAmount,
            FinalAmount = request.TotalAmount - request.DiscountAmount,
            DueDate = request.DueDate,
            CreatedSource = request.CreatedSource
        };
        var created = _invoiceService.CreateInvoice(invoice);
        return CreatedAtAction(nameof(GetById), new { invoiceId = created.Id }, created);
    }

    [HttpPost("generate-by-class/{classId}")]
    public IActionResult GenerateByClass(int classId, [FromBody] GenerateBulkInvoicesRequest request)
    {
        var invoices = _invoiceService.GenerateInvoicesByClass(classId, request.StudentIds, request.CourseId, request.TotalAmount, request.DiscountAmount, request.DueDate);
        return Created(string.Empty, invoices);
    }

    [HttpPost("{invoiceId}/cancel")]
    public IActionResult Cancel(int invoiceId)
    {
        var success = _invoiceService.CancelInvoice(invoiceId);
        if (!success) return NotFound();
        return NoContent();
    }
}

public class CreateInvoiceRequest
{
    public int StudentId { get; set; }
    public int EnrollmentId { get; set; }
    public int CourseId { get; set; }
    public int ClassId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public DateTime DueDate { get; set; }
    public string CreatedSource { get; set; } = "Manual";
}

public class GenerateBulkInvoicesRequest
{
    public IEnumerable<int> StudentIds { get; set; } = new List<int>();
    public int CourseId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public DateTime DueDate { get; set; }
}
