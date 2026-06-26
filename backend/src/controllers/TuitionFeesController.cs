using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentReport.Api.Infrastructure;
using PaymentReport.Api.Services;

namespace PaymentReport.Api.Controllers;

[ApiController]
[Route("api/payment-report/admin/tuition-fees")]
[Authorize(Roles = "Admin")]
public class TuitionFeesController : ControllerBase
{
    private readonly TuitionFeeService _tuitionFeeService;

    public TuitionFeesController(TuitionFeeService tuitionFeeService)
    {
        _tuitionFeeService = tuitionFeeService;
    }

    [HttpGet]
    public IActionResult GetAll() => Ok(_tuitionFeeService.GetTuitionFees());

    [HttpPost]
    public IActionResult Create([FromBody] TuitionFeeRequest request)
    {
        var fee = new TuitionFee
        {
            CourseId = request.CourseId,
            ClassId = request.ClassId,
            Amount = request.Amount,
            Currency = request.Currency,
            EffectiveFrom = request.EffectiveFrom,
            IsActive = request.IsActive
        };
        var created = _tuitionFeeService.CreateTuitionFee(fee);
        return CreatedAtAction(nameof(GetById), new { feeId = created.Id }, created);
    }

    [HttpGet("{feeId}")]
    public IActionResult GetById(int feeId)
    {
        var fee = _tuitionFeeService.GetTuitionFeeById(feeId);
        if (fee == null) return NotFound();
        return Ok(fee);
    }

    [HttpPut("{feeId}")]
    public IActionResult Update(int feeId, [FromBody] TuitionFeeRequest request)
    {
        var fee = new TuitionFee
        {
            CourseId = request.CourseId,
            ClassId = request.ClassId,
            Amount = request.Amount,
            Currency = request.Currency,
            EffectiveFrom = request.EffectiveFrom,
            IsActive = request.IsActive
        };
        var updated = _tuitionFeeService.UpdateTuitionFee(feeId, fee);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpGet("/api/payment-report/admin/tuition-fees/import/template")]
    public IActionResult GetTuitionFeesImportTemplate()
    {
        return Ok(new
        {
            fileName = "tuition-fees-import-template.csv",
            columns = new[] { "CourseId", "ClassId", "Amount", "Currency", "EffectiveFrom", "IsActive" }
        });
    }

    [HttpPost("/api/payment-report/admin/tuition-fees/import/preview")]
    public IActionResult PreviewTuitionFeesImport([FromBody] TuitionFeeImportRequest request)
    {
        var results = request.Items.Select(item => new
        {
            item.CourseId,
            item.ClassId,
            item.Amount,
            item.Currency,
            item.EffectiveFrom,
            item.IsActive,
            isValid = item.CourseId > 0 && item.Amount > 0 && !string.IsNullOrWhiteSpace(item.Currency),
            error = item.CourseId <= 0 || item.Amount <= 0 || string.IsNullOrWhiteSpace(item.Currency) ? "Invalid tuition fee row" : null
        });

        return Ok(new { preview = results });
    }

    [HttpPost("/api/payment-report/admin/tuition-fees/import/confirm")]
    public IActionResult ConfirmTuitionFeesImport([FromBody] TuitionFeeImportRequest request)
    {
        var createdFees = new List<TuitionFee>();
        foreach (var item in request.Items)
        {
            if (item.CourseId <= 0 || item.Amount <= 0 || string.IsNullOrWhiteSpace(item.Currency))
                continue;

            var fee = new TuitionFee
            {
                CourseId = item.CourseId,
                ClassId = item.ClassId,
                Amount = item.Amount,
                Currency = item.Currency,
                EffectiveFrom = item.EffectiveFrom,
                IsActive = item.IsActive
            };
            createdFees.Add(_tuitionFeeService.CreateTuitionFee(fee));
        }

        return Ok(new { createdFees });
    }
}

public class TuitionFeeRequest
{
    public int CourseId { get; set; }
    public int? ClassId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public DateTime EffectiveFrom { get; set; }
    public bool IsActive { get; set; } = true;
}

public class TuitionFeeImportRequest
{
    public IEnumerable<TuitionFeeImportItem> Items { get; set; } = new List<TuitionFeeImportItem>();
}

public class TuitionFeeImportItem
{
    public int CourseId { get; set; }
    public int? ClassId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public DateTime EffectiveFrom { get; set; }
    public bool IsActive { get; set; } = true;
}
