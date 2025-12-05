using Microsoft.EntityFrameworkCore;
using NextGen.Domain.Entities;

namespace NextGen.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<AppUser> Users => Set<AppUser>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>().HasKey(t => t.Id);
        modelBuilder.Entity<Account>().HasKey(a => a.Id);
        modelBuilder.Entity<Transaction>().HasKey(t => t.Id);
        modelBuilder.Entity<AppUser>().HasKey(u => u.Id);

        modelBuilder.Entity<Account>()
            .HasOne(a => a.Tenant)
            .WithMany()
            .HasForeignKey(a => a.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Account)
            .WithMany(a => a.Transactions)
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
