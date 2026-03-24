using BudgetTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<Expense> Expenses { get; set; }
    public DbSet<Budget>  Budgets  { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Expense>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.Property(x => x.Category).HasConversion<string>();
        });

        builder.Entity<Budget>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Limit).HasPrecision(18, 2);
            b.Property(x => x.Category).HasConversion<string>();
            b.HasIndex(x => new { x.Category, x.Month, x.Year }).IsUnique();
        });
    }
}
