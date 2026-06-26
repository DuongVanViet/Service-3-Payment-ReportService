using Microsoft.AspNetCore.Mvc;
using PaymentReport.Api.Infrastructure;
using PaymentReport.Api.Services;

namespace PaymentReport.Api.Controllers;

[ApiController]
[Route("api/payment-report/internal")]
public class InternalController : ControllerBase
{
    private readonly UserService _userService;
    private readonly InvoiceService _invoiceService;

    public InternalController(UserService userService, InvoiceService invoiceService)
    {
        _userService = userService;
        _invoiceService = invoiceService;
    }

    [HttpPost("users/create-student-account")]
    public IActionResult CreateStudentAccount([FromBody] CreateExternalAccountRequest request)
    {
        var user = _userService.CreateExternalUser(request.FullName, request.Email, request.Phone, request.RelatedEntityId, request.RelatedEntityType, Role.Student);
        if (user == null) return BadRequest();
        return Created(string.Empty, new { userId = user.Id, username = user.Username, temporaryPassword = "Temp@123" });
    }

    [HttpPost("users/create-teacher-account")]
    public IActionResult CreateTeacherAccount([FromBody] CreateExternalAccountRequest request)
    {
        var user = _userService.CreateExternalUser(request.FullName, request.Email, request.Phone, request.RelatedEntityId, request.RelatedEntityType, Role.Teacher);
        if (user == null) return BadRequest();
        return Created(string.Empty, new { userId = user.Id, username = user.Username, temporaryPassword = "Temp@123" });
    }

    [HttpGet("users/{userId}")]
    public IActionResult GetUser(int userId)
    {
        var user = _userService.GetUserById(userId);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpGet("students/{studentId}/debt-status")]
    public IActionResult GetDebtStatus(int studentId)
    {
        var invoices = _invoiceService.GetInvoicesByStudentId(studentId).ToList();
        var totalDebt = invoices.Sum(i => i.DebtAmount);
        var overdueCount = invoices.Count(i => i.Status == InvoiceStatus.Overdue);

        return Ok(new
        {
            studentId,
            totalDebt,
            overdueCount,
            hasOverdueDebt = overdueCount > 0
        });
    }

    [HttpPost("invoices/generate-from-enrollment")]
    public IActionResult GenerateInvoiceFromEnrollment([FromBody] GenerateInvoiceRequest request)
    {
        var invoice = _invoiceService.CreateInvoiceFromEnrollment(request.StudentId, request.EnrollmentId, request.CourseId, request.ClassId, request.TotalAmount, request.DiscountAmount, request.DueDate);
        if (invoice == null) return BadRequest(new { message = "Invoice already exists for enrollment" });
        return Created(string.Empty, invoice);
    }

    [HttpPost("invoices/generate-bulk-from-enrollments")]
    public IActionResult GenerateBulkInvoiceFromEnrollments([FromBody] GenerateBulkInvoiceRequest request)
    {
        var invoices = _invoiceService.GenerateInvoicesByClass(request.ClassId, request.StudentIds, request.CourseId, request.TotalAmount, request.DiscountAmount, request.DueDate);
        return Created(string.Empty, invoices);
    }

    [HttpGet("invoices/by-enrollment/{enrollmentId}")]
    public IActionResult GetInvoiceByEnrollment(int enrollmentId)
    {
        var invoice = _invoiceService.GetInvoiceByEnrollment(enrollmentId);
        if (invoice == null) return NotFound();
        return Ok(invoice);
    }
}

public class CreateExternalAccountRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int RelatedEntityId { get; set; }
    public string RelatedEntityType { get; set; } = string.Empty;
}

public class GenerateInvoiceRequest
{
    public int StudentId { get; set; }
    public int EnrollmentId { get; set; }
    public int CourseId { get; set; }
    public int ClassId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public DateTime DueDate { get; set; }
}

public class GenerateBulkInvoiceRequest
{
    public IEnumerable<int> StudentIds { get; set; } = new List<int>();
    public int ClassId { get; set; }
    public int CourseId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public DateTime DueDate { get; set; }
}
