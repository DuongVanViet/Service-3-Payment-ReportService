namespace PaymentReport.Api.Commands;

public class CreateUserCommand
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int RelatedEntityId { get; set; }
    public string RelatedEntityType { get; set; } = string.Empty;
    public bool MustChangePassword { get; set; } = true;
    public string Password { get; set; } = "Temp@123";
}
