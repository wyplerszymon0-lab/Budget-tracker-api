using BudgetTracker.Data;
using BudgetTracker.DTOs;
using BudgetTracker.Models;
using BudgetTracker.Services;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Tests;

public class ExpenseServiceTests : IDisposable
{
    private readonly AppDbContext    _db;
    private readonly ExpenseService  _service;

    public ExpenseServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db      = new AppDbContext(options);
        _service = new ExpenseService(_db);
    }

    public void Dispose() => _db.Dispose();

    private CreateExpenseRequest MakeRequest(
        string title    = "Coffee",
        decimal amount  = 5.00m,
        ExpenseCategory cat = ExpenseCategory.Food) =>
        new(title, amount, cat, "", DateTime.UtcNow);

    [Fact]
    public async Task CreateAsync_ReturnsCreatedExpense()
    {
        var req    = MakeRequest("Coffee", 5.00m);
        var result = await _service.CreateAsync(req);

        Assert.Equal("Coffee",             result.Title);
        Assert.Equal(5.00m,                result.Amount);
        Assert.Equal("Food",               result.Category);
        Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsExpense_WhenExists()
    {
        var created = await _service.CreateAsync(MakeRequest());
        var found   = await _service.GetByIdAsync(created.Id);

        Assert.NotNull(found);
        Assert.Equal(created.Id, found.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _service.GetByIdAsync(999);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllExpenses()
    {
        await _service.CreateAsync(MakeRequest("A"));
        await _service.CreateAsync(MakeRequest("B"));
        await _service.CreateAsync(MakeRequest("C"));

        var all = await _service.GetAllAsync(null, null, null);

        Assert.Equal(3, all.Count);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByCategory()
    {
        await _service.CreateAsync(MakeRequest(cat: ExpenseCategory.Food));
        await _service.CreateAsync(MakeRequest(cat: ExpenseCategory.Transport));
        await _service.CreateAsync(MakeRequest(cat: ExpenseCategory.Food));

        var food = await _service.GetAllAsync(ExpenseCategory.Food, null, null);

        Assert.Equal(2, food.Count);
        Assert.All(food, e => Assert.Equal("Food", e.Category));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesFields()
    {
        var created = await _service.CreateAsync(MakeRequest("Old Title", 10m));
        var updated = await _service.UpdateAsync(created.Id, new UpdateExpenseRequest("New Title", 20m, null, null, null));

        Assert.NotNull(updated);
        Assert.Equal("New Title", updated.Title);
        Assert.Equal(20m,         updated.Amount);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _service.UpdateAsync(999, new UpdateExpenseRequest("X", null, null, null, null));
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrueAndRemoves()
    {
        var created = await _service.CreateAsync(MakeRequest());
        var deleted = await _service.DeleteAsync(created.Id);
        var found   = await _service.GetByIdAsync(created.Id);

        Assert.True(deleted);
        Assert.Null(found);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
    {
        var result = await _service.DeleteAsync(999);
        Assert.False(result);
    }

    [Fact]
    public async Task GetSummaryAsync_ReturnsCorrectTotals()
    {
        await _service.CreateAsync(MakeRequest(amount: 10m, cat: ExpenseCategory.Food));
        await _service.CreateAsync(MakeRequest(amount: 20m, cat: ExpenseCategory.Food));
        await _service.CreateAsync(MakeRequest(amount: 30m, cat: ExpenseCategory.Transport));

        var summary = await _service.GetSummaryAsync(null, null);

        Assert.Equal(60m, summary.TotalAmount);
        Assert.Equal(3,   summary.TotalCount);
        Assert.Equal(20m, summary.AverageAmount);
        Assert.Equal(30m, summary.MaxAmount);
        Assert.Equal(10m, summary.MinAmount);
        Assert.Equal(2,   summary.ByCategory.Count);
    }

    [Fact]
    public async Task GetSummaryAsync_ReturnsEmpty_WhenNoExpenses()
    {
        var summary = await _service.GetSummaryAsync(null, null);

        Assert.Equal(0, summary.TotalAmount);
        Assert.Equal(0, summary.TotalCount);
        Assert.Empty(summary.ByCategory);
    }

    [Fact]
    public async Task GetMonthlyReportAsync_IncludesBudgetStatus()
    {
        var month = DateTime.UtcNow.Month;
        var year  = DateTime.UtcNow.Year;

        var req = new CreateExpenseRequest("Lunch", 50m, ExpenseCategory.Food, "", new DateTime(year, month, 1));
        await _service.CreateAsync(req);

        _db.Budgets.Add(new Budget
        {
            Category  = ExpenseCategory.Food,
            Limit     = 100m,
            Month     = month,
            Year      = year,
            CreatedAt = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();

        var report = await _service.GetMonthlyReportAsync(month, year);

        Assert.Equal(50m, report.TotalSpent);
        Assert.Single(report.BudgetStatuses);
        Assert.Equal(50m,  report.BudgetStatuses[0].Spent);
        Assert.Equal(50m,  report.BudgetStatuses[0].Remaining);
        Assert.False(report.BudgetStatuses[0].IsOverBudget);
    }
}
