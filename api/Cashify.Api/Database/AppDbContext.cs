using Cashify.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cashify.Api.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<BusinessMember> BusinessMembers => Set<BusinessMember>();
    public DbSet<Cashbook> Cashbooks => Set<Cashbook>();
    public DbSet<CashbookMember> CashbookMembers => Set<CashbookMember>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<TransactionChange> TransactionChanges => Set<TransactionChange>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.GoogleUserId)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<BusinessMember>()
            .HasIndex(x => new { x.BusinessId, x.UserId })
            .IsUnique();

        modelBuilder.Entity<CashbookMember>()
            .HasIndex(x => new { x.CashbookId, x.UserId })
            .IsUnique();

        modelBuilder.Entity<Transaction>()
            .HasIndex(x => new { x.BusinessId, x.CashbookId, x.TransactionDate });

        modelBuilder.Entity<TransactionChange>()
            .Property(x => x.ChangesJson)
            .HasColumnType("jsonb");
    }
}

