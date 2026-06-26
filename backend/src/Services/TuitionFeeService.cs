using PaymentReport.Api.Infrastructure;

namespace PaymentReport.Api.Services;

public class TuitionFeeService
{
    private readonly AppDbContext _db;

    public TuitionFeeService(AppDbContext db)
    {
        _db = db;
    }

    public IEnumerable<TuitionFee> GetTuitionFees() => _db.TuitionFees.ToList();

    public TuitionFee? GetTuitionFeeById(int feeId) => _db.TuitionFees.Find(feeId);

    public IEnumerable<TuitionFee> GetActiveTuitionFees() => _db.TuitionFees.Where(f => f.IsActive).ToList();

    public TuitionFee CreateTuitionFee(TuitionFee fee)
    {
        fee.CreatedAt = DateTime.UtcNow;
        _db.TuitionFees.Add(fee);
        _db.SaveChanges();
        return fee;
    }

    public TuitionFee? UpdateTuitionFee(int feeId, TuitionFee updatedFee)
    {
        var fee = GetTuitionFeeById(feeId);
        if (fee == null) return null;
        fee.CourseId = updatedFee.CourseId;
        fee.ClassId = updatedFee.ClassId;
        fee.Amount = updatedFee.Amount;
        fee.Currency = updatedFee.Currency;
        fee.EffectiveFrom = updatedFee.EffectiveFrom;
        fee.IsActive = updatedFee.IsActive;
        _db.SaveChanges();
        return fee;
    }
}
