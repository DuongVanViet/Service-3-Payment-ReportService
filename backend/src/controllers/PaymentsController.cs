using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentReport.Api.Infrastructure;
using PaymentReport.Api.Services;

namespace PaymentReport.Api.Controllers;

[ApiController]
[Route("api/payment-report/admin/invoices")]
[Authorize(Roles = "Admin")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentService _paymentService;
    private readonly InvoiceService _invoiceService;

    public PaymentsController(PaymentService paymentService, InvoiceService invoiceService)
    {
        _paymentService = paymentService;
        _invoiceService = invoiceService;
    }

    [HttpPost("{invoiceId}/payments")]
    public IActionResult CreatePayment(int invoiceId, [FromBody] CreatePaymentRequest request)
    {
        var payment = new PaymentTransaction
        {
            InvoiceId = invoiceId,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            CollectedBy = request.CollectedBy,
            Note = request.Note
        };
        var created = _paymentService.CreatePayment(payment);
        if (created == null) return NotFound(new { message = "Invoice not found" });
        return CreatedAtAction(nameof(GetById), new { invoiceId, paymentId = created.Id }, created);
    }

    [HttpGet("{invoiceId}/payments")]
    public IActionResult GetPayments(int invoiceId)
    {
        return Ok(_paymentService.GetPaymentsByInvoice(invoiceId));
    }

    [HttpGet("payments/{paymentId}")]
    public IActionResult GetById(int paymentId)
    {
        var payment = _paymentService.GetPaymentById(paymentId);
        if (payment == null) return NotFound();
        return Ok(payment);
    }

    [HttpGet("/api/payment-report/admin/receipts/{paymentId}")]
    public IActionResult GetReceipt(int paymentId)
    {
        var payment = _paymentService.GetPaymentById(paymentId);
        if (payment == null) return NotFound();

        var invoice = _invoiceService.GetInvoiceById(payment.InvoiceId);
        return Ok(new
        {
            receipt = new
            {
                payment.Id,
                payment.InvoiceId,
                payment.Amount,
                payment.PaymentMethod,
                payment.PaidAt,
                payment.CollectedBy,
                payment.Note
            },
            invoice = invoice == null ? null : new
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
                status = invoice.Status.ToString()
            }
        });
    }

    [HttpGet("/api/payment-report/admin/payments/import/template")]
    public IActionResult GetPaymentsImportTemplate()
    {
        return Ok(new
        {
            fileName = "payments-import-template.csv",
            columns = new[] { "InvoiceId", "Amount", "PaymentMethod", "CollectedBy", "Note" }
        });
    }

    [HttpPost("/api/payment-report/admin/payments/import/preview")]
    public IActionResult PreviewPaymentsImport([FromBody] PaymentImportRequest request)
    {
        var results = request.Items.Select(item => new
        {
            item.InvoiceId,
            item.Amount,
            item.PaymentMethod,
            item.CollectedBy,
            item.Note,
            isValid = item.InvoiceId > 0 && item.Amount > 0 && !string.IsNullOrWhiteSpace(item.PaymentMethod),
            error = item.InvoiceId <= 0 || item.Amount <= 0 || string.IsNullOrWhiteSpace(item.PaymentMethod) ? "Invalid payment data" : null
        });

        return Ok(new { preview = results });
    }

    [HttpPost("/api/payment-report/admin/payments/import/confirm")]
    public IActionResult ConfirmPaymentsImport([FromBody] PaymentImportRequest request)
    {
        var createdPayments = new List<PaymentTransaction>();
        foreach (var item in request.Items)
        {
            if (item.InvoiceId <= 0 || item.Amount <= 0 || string.IsNullOrWhiteSpace(item.PaymentMethod))
                continue;

            var payment = new PaymentTransaction
            {
                InvoiceId = item.InvoiceId,
                Amount = item.Amount,
                PaymentMethod = item.PaymentMethod,
                CollectedBy = item.CollectedBy,
                Note = item.Note
            };

            var created = _paymentService.CreatePayment(payment);
            if (created != null)
                createdPayments.Add(created);
        }

        return Ok(new { createdPayments });
    }
}

public class CreatePaymentRequest
{
    public int? InvoiceId { get; set; }  // Optional: for admin endpoint
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string CollectedBy { get; set; } = string.Empty;
    public string? Note { get; set; }
}

public class PaymentImportRequest
{
    public IEnumerable<PaymentImportItem> Items { get; set; } = new List<PaymentImportItem>();
}

public class PaymentImportItem
{
    public int InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string CollectedBy { get; set; } = string.Empty;
    public string? Note { get; set; }
}
