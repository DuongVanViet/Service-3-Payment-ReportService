using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentReport.Api.Infrastructure;
using PaymentReport.Api.Services;

namespace PaymentReport.Api.Controllers;

[ApiController]
[Route("api/payment-report/teacher")]
[Authorize(Roles = "Teacher")]
public class TeacherController : ControllerBase
{
    private readonly InvoiceService _invoiceService;

    public TeacherController(InvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpGet("dashboard-summary")]
    public IActionResult DashboardSummary()
    {
        var invoices = _invoiceService.GetInvoices().ToList();
        var classSummaries = invoices.GroupBy(i => i.ClassId)
            .Select(g => new
            {
                classId = g.Key,
                invoiceCount = g.Count(),
                totalDebt = g.Sum(i => i.DebtAmount),
                overdueCount = g.Count(i => i.Status == InvoiceStatus.Overdue)
            })
            .OrderByDescending(g => g.totalDebt)
            .ToList();

        return Ok(new
        {
            totalClasses = classSummaries.Count,
            totalInvoices = invoices.Count,
            totalDebt = invoices.Sum(i => i.DebtAmount),
            overdueInvoiceCount = invoices.Count(i => i.Status == InvoiceStatus.Overdue),
            classSummaries
        });
    }

    [HttpGet("reports/classes")]
    public IActionResult ClassesReport()
    {
        var invoices = _invoiceService.GetInvoices().ToList();
        var report = invoices.GroupBy(i => i.ClassId)
            .Select(g => new
            {
                classId = g.Key,
                invoiceCount = g.Count(),
                totalAmount = g.Sum(i => i.TotalAmount),
                totalPaid = g.Sum(i => i.PaidAmount),
                totalDebt = g.Sum(i => i.DebtAmount),
                overdueCount = g.Count(i => i.Status == InvoiceStatus.Overdue)
            })
            .OrderByDescending(g => g.totalDebt)
            .ToList();

        return Ok(report);
    }

    [HttpGet("reports/classes/{classId}")]
    public IActionResult ClassReport(int classId)
    {
        var invoices = _invoiceService.GetInvoicesByClassId(classId).ToList();
        if (!invoices.Any()) return NotFound(new { message = "Class not found or no invoice data" });

        return Ok(new
        {
            classId,
            invoiceCount = invoices.Count,
            totalAmount = invoices.Sum(i => i.TotalAmount),
            totalPaid = invoices.Sum(i => i.PaidAmount),
            totalDebt = invoices.Sum(i => i.DebtAmount),
            overdueCount = invoices.Count(i => i.Status == InvoiceStatus.Overdue),
            studentInvoices = invoices.GroupBy(i => i.StudentId)
                .Select(g => new
                {
                    studentId = g.Key,
                    invoiceCount = g.Count(),
                    totalDebt = g.Sum(i => i.DebtAmount),
                    overdueCount = g.Count(i => i.Status == InvoiceStatus.Overdue)
                })
                .ToList()
        });
    }

    [HttpGet("reports/classes/{classId}/attendance")]
    public IActionResult ClassAttendance(int classId)
    {
        return Ok(new
        {
            classId,
            message = "Attendance details are not stored in the Payment & Report Service. Use Student & Attendance Service for attendance reports."
        });
    }

    [HttpGet("reports/classes/{classId}/results")]
    public IActionResult ClassResults(int classId)
    {
        return Ok(new
        {
            classId,
            message = "Result details are not stored in the Payment & Report Service. Use Student & Attendance Service for class results."
        });
    }
}
