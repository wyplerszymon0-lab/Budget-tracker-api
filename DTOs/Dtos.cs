using System.ComponentModel.DataAnnotations;
using BudgetTracker.Models;

namespace BudgetTracker.DTOs;

public record CreateExpenseRequest(
    [Required, MinLength(1), MaxLength(200)]
    string Title,

    [Range(0.01, 1_000_000)]
    decimal Amount,

    ExpenseCategory Category,

    string Note,

    DateTime? Date
);

public record UpdateExpenseRequest(
    [MinLength(1), MaxLength(200)]
    string? Title,

    [Range(0.01, 1_000_000)]
    decimal? Amount,

    ExpenseCategory? Category,

    string? Note,

    DateTime? Date
);

public record ExpenseResponse(
    int             Id,
    string          Title,
    decimal         Amount,
    string          Category,
    string          Note,
    DateTime        Date,
    DateTime        CreatedAt
);

public record ExpenseSummary(
    decimal               TotalAmount,
    int                   TotalCount,
    decimal               AverageAmount,
    decimal               MaxAmount,
    decimal               MinAmount,
    List<CategorySummary> ByCategory
);

public record CategorySummary(
    string  Category,
    decimal TotalAmount,
    int     Count,
    decimal Percentage
);

public record MonthlyReport(
    int                   Month,
    int                   Year,
    decimal               TotalSpent,
    List<CategorySummary> ByCategory,
    List<BudgetStatus>    BudgetStatuses
);

public record BudgetStatus(
    string  Category,
    decimal Limit,
    decimal Spent,
    decimal Remaining,
    decimal UsedPercentage,
    bool    IsOverBudget
);
