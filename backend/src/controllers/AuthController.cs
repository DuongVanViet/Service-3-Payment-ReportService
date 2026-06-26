using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentReport.Api.Infrastructure;
using PaymentReport.Api.Services;

namespace PaymentReport.Api.Controllers;

[ApiController]
[Route("api/payment-report/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly UserService _userService;

    public AuthController(AuthService authService, UserService userService)
    {
        _authService = authService;
        _userService = userService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var user = _authService.Authenticate(request.Username, request.Password);
        if (user == null)
            return Unauthorized(new { message = "Invalid credentials" });

        var accessToken = _authService.GenerateToken(user);
        var refreshToken = _authService.GenerateRefreshToken(user.Id);

        return Ok(new
        {
            accessToken,
            refreshToken,
            user = new
            {
                userId = user.Id,
                fullName = user.FullName,
                email = user.Email,
                role = user.Role.ToString(),
                relatedEntityId = user.RelatedEntityId,
                relatedEntityType = user.RelatedEntityType,
                mustChangePassword = user.MustChangePassword
            }
        });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var user = _userService.GetUserById(userId.Value);
        if (user == null) return NotFound();

        return Ok(new
        {
            userId = user.Id,
            username = user.Username,
            email = user.Email,
            fullName = user.FullName,
            role = user.Role.ToString(),
            relatedEntityId = user.RelatedEntityId,
            relatedEntityType = user.RelatedEntityType,
            mustChangePassword = user.MustChangePassword,
            status = user.Status.ToString(),
            createdAt = user.CreatedAt,
            updatedAt = user.UpdatedAt
        });
    }

    [Authorize]
    [HttpPost("change-password")]
    public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var success = _userService.ChangePassword(userId.Value, request.CurrentPassword, request.NewPassword);
        if (!success) return BadRequest(new { message = "Password update failed" });
        return NoContent();
    }

    [HttpPost("refresh-token")]
    public IActionResult RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var refreshToken = _authService.ValidateRefreshToken(request.RefreshToken);
        if (refreshToken == null) return Unauthorized(new { message = "Invalid refresh token" });

        var user = _authService.GetUserById(refreshToken.UserId);
        if (user == null) return Unauthorized(new { message = "Invalid refresh token" });

        _authService.RevokeRefreshToken(request.RefreshToken);
        var newRefreshToken = _authService.GenerateRefreshToken(user.Id);
        var accessToken = _authService.GenerateToken(user);

        return Ok(new { accessToken, refreshToken = newRefreshToken });
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout([FromBody] RefreshTokenRequest request)
    {
        var success = _authService.RevokeRefreshToken(request.RefreshToken);
        if (!success) return BadRequest(new { message = "Invalid refresh token" });
        return NoContent();
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdClaim, out var userId)) return userId;
        return null;
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
