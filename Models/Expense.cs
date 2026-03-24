namespace BudgetTracker.Models;

public enum ExpenseCategory
{
    Food,
    Transport,
    Housing,
    Entertainment,
    Health,
    Education,
    Shopping,
    Other,
}

public class Expense
{
    public int             Id          { get; set; }
    public string          Title       { get; set; } = string.Empty;
    public decimal         Amount      { get; set; }
    public ExpenseCategory Category    { get; set; }
    public string          Note        { get; set; } = string.Empty;
    public DateTime        Date        { get; set; }
    public DateTime        CreatedAt   { get; set; }
    public DateTime        UpdatedAt   { get; set; }
}
