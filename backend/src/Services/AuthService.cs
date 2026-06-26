using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PaymentReport.Api.Infrastructure;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PaymentReport.Api.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly string _jwtSecret;

    public AuthService(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _jwtSecret = configuration.GetValue<string>("JwtSecret") ?? "super-secret-payment-report-service";
    }

    public UserAccount? Authenticate(string username, string password)
    {
        var normalizedUsername = username.ToLowerInvariant();
        var user = _db.Users.FirstOrDefault(u => u.Username.ToLower() == normalizedUsername);
        if (user == null) return null;
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;
        return user;
    }

    public UserAccount? GetUserById(int userId) => _db.Users.Find(userId);

    public string GenerateToken(UserAccount user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("relatedEntityId", user.RelatedEntityId.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("mustChangePassword", user.MustChangePassword.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(4),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken(int userId)
    {
        var token = Guid.NewGuid().ToString("N");
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            Revoked = false,
            CreatedAt = DateTime.UtcNow
        };
        _db.RefreshTokens.Add(refreshToken);
        _db.SaveChanges();
        return token;
    }

    public RefreshToken? ValidateRefreshToken(string token)
    {
        var refreshToken = _db.RefreshTokens.FirstOrDefault(t => t.Token == token && !t.Revoked);
        if (refreshToken == null) return null;
        if (refreshToken.ExpiresAt < DateTime.UtcNow) return null;
        return refreshToken;
    }

    public bool RevokeRefreshToken(string token)
    {
        var refreshToken = _db.RefreshTokens.FirstOrDefault(t => t.Token == token);
        if (refreshToken == null) return false;
        refreshToken.Revoked = true;
        _db.SaveChanges();
        return true;
    }

    public bool RevokeAllRefreshTokens(int userId)
    {
        var tokens = _db.RefreshTokens.Where(t => t.UserId == userId && !t.Revoked).ToList();
        if (!tokens.Any()) return false;
        foreach (var token in tokens)
        {
            token.Revoked = true;
        }
        _db.SaveChanges();
        return true;
    }
}
