using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentReport.Api.Infrastructure;
using PaymentReport.Api.Services;

namespace PaymentReport.Api.Controllers;

[ApiController]
[Route("api/payment-report/admin/users")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_userService.GetAllUsers());
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateUserRequest request)
    {
        var user = new UserAccount
        {
            Username = request.Username,
            Email = request.Email,
            Phone = request.Phone,
            FullName = request.FullName,
            Role = Enum.Parse<Role>(request.Role, true),
            RelatedEntityId = request.RelatedEntityId,
            RelatedEntityType = request.RelatedEntityType,
            MustChangePassword = request.MustChangePassword,
            PasswordHash = request.Password
        };

        var created = _userService.CreateUser(user);
        return CreatedAtAction(nameof(GetById), new { userId = created.Id }, created);
    }

    [HttpGet("{userId}")]
    public IActionResult GetById(int userId)
    {
        var user = _userService.GetUserById(userId);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPatch("{userId}/status")]
    public IActionResult UpdateStatus(int userId, [FromBody] UpdateStatusRequest request)
    {
        var success = _userService.UpdateUserStatus(userId, Enum.Parse<UserStatus>(request.Status, true));
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpPost("{userId}/reset-password")]
    public IActionResult ResetPassword(int userId)
    {
        var success = _userService.ResetPassword(userId);
        if (!success) return NotFound();
        return NoContent();
    }
}

public class CreateUserRequest
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

public class UpdateStatusRequest
{
    public string Status { get; set; } = string.Empty;
}
