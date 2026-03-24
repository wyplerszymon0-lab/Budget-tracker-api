using BudgetTracker.Data;
using BudgetTracker.DTOs;
using BudgetTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Services;

public interface IExpenseService
{
    Task<List<ExpenseResponse>>  GetAllAsync(ExpenseCategory? category, int? month, int? year);
    Task<ExpenseResponse?>       GetByIdAsync(int id);
    Task<ExpenseResponse>        CreateAsync(CreateExpenseRequest request);
    Task<ExpenseResponse?>       UpdateAsync(int id, UpdateExpenseRequest request);
    Task<bool>                   DeleteAsync(int id);
    Task<ExpenseSummary>         GetSummaryAsync(int? month, int? year);
    Task<MonthlyReport>          GetMonthlyReportAsync(int month, int year);
}

public class ExpenseService : IExpenseService
{
    private readonly AppDbContext _db;

    public ExpenseService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ExpenseResponse>> GetAllAsync(ExpenseCategory? category, int? month, int? year)
    {
        var query = _db.Expenses.AsQueryable();

        if (category.HasValue) query = query.Where(e => e.Category == category.Value);
        if (month.HasValue)    query = query.Where(e => e.Date.Month == month.Value);
        if (year.HasValue)     query = query.Where(e => e.Date.Year == year.Value);

        return await query
            .OrderByDescending(e => e.Date)
            .Select(e => ToResponse(e))
            .ToListAsync();
    }

    public async Task<ExpenseResponse?> GetByIdAsync(int id)
    {
        var expense = await _db.Expenses.FindAsync(id);
        return expense is null ? null : ToResponse(expense);
    }

    public async Task<ExpenseResponse> CreateAsync(CreateExpenseRequest req)
    {
        var expense = new Expense
        {
            Title     = req.Title,
            Amount    = req.Amount,
            Category  = req.Category,
            Note      = req.Note ?? string.Empty,
            Date      = req.Date?.ToUniversalTime() ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _db.Expenses.Add(expense);
        await _db.SaveChangesAsync();
        return ToResponse(expense);
    }

    public async Task<ExpenseResponse?> UpdateAsync(int id, UpdateExpenseRequest req)
    {
        var expense = await _db.Expenses.FindAsync(id);
        if (expense is null) return null;

        if (req.Title    is not null) expense.Title    = req.Title;
        if (req.Amount   is not null) expense.Amount   = req.Amount.Value;
        if (req.Category is not null) expense.Category = req.Category.Value;
        if (req.Note     is not null) expense.Note     = req.Note;
        if (req.Date     is not null) expense.Date     = req.Date.Value.ToUniversalTime();

        expense.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ToResponse(expense);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var expense = await _db.Expenses.FindAsync(id);
        if (expense is null) return false;

        _db.Expenses.Remove(expense);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<ExpenseSummary> GetSummaryAsync(int? month, int? year)
    {
        var query = _db.Expenses.AsQueryable();

        if (month.HasValue) query = query.Where(e => e.Date.Month == month.Value);
        if (year.HasValue)  query = query.Where(e => e.Date.Year == year.Value);

        var expenses = await query.ToListAsync();

        if (expenses.Count == 0)
        {
            return new ExpenseSummary(0, 0, 0, 0, 0, []);
        }

        var total   = expenses.Sum(e => e.Amount);
        var byCategory = expenses
            .GroupBy(e => e.Category)
            .Select(g => new CategorySummary(
                g.Key.ToString(),
                g.Sum(e => e.Amount),
                g.Count(),
                total > 0 ? Math.Round(g.Sum(e => e.Amount) / total * 100, 1) : 0
            ))
            .OrderByDescending(c => c.TotalAmount)
            .ToList();

        return new ExpenseSummary(
            TotalAmount:   total,
            TotalCount:    expenses.Count,
            AverageAmount: Math.Round(expenses.Average(e => e.Amount), 2),
            MaxAmount:     expenses.Max(e => e.Amount),
            MinAmount:     expenses.Min(e => e.Amount),
            ByCategory:    byCategory
        );
    }

    public async Task<MonthlyReport> GetMonthlyReportAsync(int month, int year)
    {
        var expenses = await _db.Expenses
            .Where(e => e.Date.Month == month && e.Date.Year == year)
            .ToListAsync();

        var budgets = await _db.Budgets
            .Where(b => b.Month == month && b.Year == year)
            .ToListAsync();

        var total      = expenses.Sum(e => e.Amount);
        var byCategory = expenses
            .GroupBy(e => e.Category)
            .Select(g => new CategorySummary(
                g.Key.ToString(),
                g.Sum(e => e.Amount),
                g.Count(),
                total > 0 ? Math.Round(g.Sum(e => e.Amount) / total * 100, 1) : 0
            ))
            .OrderByDescending(c => c.TotalAmount)
            .ToList();

        var budgetStatuses = budgets.Select(b =>
        {
            var spent     = expenses.Where(e => e.Category == b.Category).Sum(e => e.Amount);
            var remaining = b.Limit - spent;
            return new BudgetStatus(
                Category:      b.Category.ToString(),
                Limit:         b.Limit,
                Spent:         spent,
                Remaining:     remaining,
                UsedPercentage: b.Limit > 0 ? Math.Round(spent / b.Limit * 100, 1) : 0,
                IsOverBudget:  spent > b.Limit
            );
        }).ToList();

        return new MonthlyReport(month, year, total, byCategory, budgetStatuses);
    }

    private static ExpenseResponse ToResponse(Expense e) =>
        new(e.Id, e.Title, e.Amount, e.Category.ToString(), e.Note, e.Date, e.CreatedAt);
}
