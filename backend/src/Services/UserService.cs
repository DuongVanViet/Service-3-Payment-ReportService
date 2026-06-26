using PaymentReport.Api.Infrastructure;

namespace PaymentReport.Api.Services;

public class UserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db)
    {
        _db = db;
    }

    public IEnumerable<UserAccount> GetAllUsers() => _db.Users.ToList();

    public UserAccount? GetUserById(int userId) => _db.Users.Find(userId);

    public UserAccount CreateUser(UserAccount user)
    {
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        _db.Users.Add(user);
        _db.SaveChanges();
        return user;
    }

    public UserAccount? CreateExternalUser(string fullName, string email, string? phone, int relatedEntityId, string relatedEntityType, Role role)
    {
        var newUser = new UserAccount
        {
            Username = email,
            Email = email,
            Phone = phone,
            FullName = fullName,
            Role = role,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType,
            MustChangePassword = true,
            Status = UserStatus.Active,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Temp@123"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Users.Add(newUser);
        _db.SaveChanges();
        return newUser;
    }

    public bool ChangePassword(int userId, string currentPassword, string newPassword)
    {
        var user = GetUserById(userId);
        if (user == null) return false;
        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash)) return false;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.MustChangePassword = false;
        user.UpdatedAt = DateTime.UtcNow;
        _db.SaveChanges();
        return true;
    }

    public bool UpdateUserStatus(int userId, UserStatus status)
    {
        var user = GetUserById(userId);
        if (user == null) return false;
        user.Status = status;
        user.UpdatedAt = DateTime.UtcNow;
        _db.SaveChanges();
        return true;
    }

    public bool ResetPassword(int userId)
    {
        var user = GetUserById(userId);
        if (user == null) return false;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Temp@123");
        user.MustChangePassword = true;
        user.UpdatedAt = DateTime.UtcNow;
        _db.SaveChanges();
        return true;
    }
}
