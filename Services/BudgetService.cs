using BudgetTracker.Data;
using BudgetTracker.DTOs;
using BudgetTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Services;

public interface IBudgetService
{
    Task<List<BudgetResponse>> GetAllAsync(int? month, int? year);
    Task<BudgetResponse?>      GetByIdAsync(int id);
    Task<BudgetResponse>       CreateAsync(CreateBudgetRequest request);
    Task<bool>                 DeleteAsync(int id);
}

public class BudgetService : IBudgetService
{
    private readonly AppDbContext _db;

    public BudgetService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<BudgetResponse>> GetAllAsync(int? month, int? year)
    {
        var query = _db.Budgets.AsQueryable();

        if (month.HasValue) query = query.Where(b => b.Month == month.Value);
        if (year.HasValue)  query = query.Where(b => b.Year == year.Value);

        return await query
            .OrderBy(b => b.Year)
            .ThenBy(b => b.Month)
            .Select(b => ToResponse(b))
            .ToListAsync();
    }

    public async Task<BudgetResponse?> GetByIdAsync(int id)
    {
        var budget = await _db.Budgets.FindAsync(id);
        return budget is null ? null : ToResponse(budget);
    }

    public async Task<BudgetResponse> CreateAsync(CreateBudgetRequest req)
    {
        var existing = await _db.Budgets.FirstOrDefaultAsync(b =>
            b.Category == req.Category &&
            b.Month    == req.Month    &&
            b.Year     == req.Year);

        if (existing is not null)
        {
            existing.Limit = req.Limit;
            await _db.SaveChangesAsync();
            return ToResponse(existing);
        }

        var budget = new Budget
        {
            Category  = req.Category,
            Limit     = req.Limit,
            Month     = req.Month,
            Year      = req.Year,
            CreatedAt = DateTime.UtcNow,
        };

        _db.Budgets.Add(budget);
        await _db.SaveChangesAsync();
        return ToResponse(budget);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var budget = await _db.Budgets.FindAsync(id);
        if (budget is null) return false;

        _db.Budgets.Remove(budget);
        await _db.SaveChangesAsync();
        return true;
    }

    private static BudgetResponse ToResponse(Budget b) =>
        new(b.Id, b.Category.ToString(), b.Limit, b.Month, b.Year);
}
