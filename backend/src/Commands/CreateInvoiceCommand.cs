namespace PaymentReport.Api.Commands;

public class CreateInvoiceCommand
{
    public int StudentId { get; set; }
    public int EnrollmentId { get; set; }
    public int CourseId { get; set; }
    public int ClassId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public DateTime DueDate { get; set; }
}
