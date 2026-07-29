# budget-tracker-api

REST API for tracking expenses and monthly budgets. Built in C# and .NET 8 with Entity Framework Core.

## Endpoints

### Expenses
| Method | Path | Description |
|---|---|---|
| GET | `/api/expenses` | Expense list (filters: category, month, year) |
| GET | `/api/expenses/{id}` | Expense details |
| POST | `/api/expenses` | Add expense |
| PATCH | `/api/expenses/{id}` | Update expense |
| DELETE | `/api/expenses/{id}` | Delete expense |
| GET | `/api/expenses/summary` | Summary by category |
| GET | `/api/expenses/report/{year}/{month}` | Monthly report with budgets |

### Budgets
| Method | Path | Description |
|---|---|---|
| GET | `/api/budgets` | Budget list |
| GET | `/api/budgets/{id}` | Budget details |
| POST | `/api/budgets` | Set budget for category and month |
| DELETE | `/api/budgets/{id}` | Delete budget |

## Categories
`Food`, `Transport`, `Housing`, `Entertainment`, `Health`, `Education`, `Shopping`, `Other`

## Rune
```bash
dotnet run
# Swagger UI: https://localhost:5001/swagger
```

## Test
```bash
dotnet test
```

## Structure
```
budget-tracker-api/
├── Controllers/
│ ├── ExpensesController.cs
│ └── BudgetsController.cs
├── Services/
│ ├── ExpenseService.cs
│ └── BudgetService.cs
├── Models/
│ ├── Expense.cs
│ └── Budget.cs
├── DTOs/
│ ├── ExpenseDtos.cs
│ └── BudgetDtos.cs
├── Data/
│ └── AppDbContext.cs
├── Program.cs
├── BudgetTracker.csproj
└── Tests/ 
├── ExpenseServiceTests.cs 
└── BudgetTracker.Tests.csproj
```

## Author

**Szymon Wypler** 
