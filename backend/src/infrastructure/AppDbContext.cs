using Microsoft.EntityFrameworkCore;

namespace PaymentReport.Api.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<UserAccount> Users { get; set; } = null!;
    public DbSet<TuitionFee> TuitionFees { get; set; } = null!;
    public DbSet<Invoice> Invoices { get; set; } = null!;
    public DbSet<PaymentTransaction> Payments { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAccount>().HasKey(e => e.Id);
        modelBuilder.Entity<UserAccount>().Property(e => e.Role).HasConversion<string>();
        modelBuilder.Entity<UserAccount>().Property(e => e.Status).HasConversion<string>();

        modelBuilder.Entity<Invoice>().Property(e => e.Status).HasConversion<string>();
        modelBuilder.Entity<RefreshToken>().HasKey(e => e.Id);
    }
}
