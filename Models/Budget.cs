namespace BudgetTracker.Models;

public class Budget
{
    public int             Id        { get; set; }
    public ExpenseCategory Category  { get; set; }
    public decimal         Limit     { get; set; }
    public int             Month     { get; set; }
    public int             Year      { get; set; }
    public DateTime        CreatedAt { get; set; }
}
