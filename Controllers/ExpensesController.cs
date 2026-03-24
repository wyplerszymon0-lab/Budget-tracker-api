using BudgetTracker.DTOs;
using BudgetTracker.Models;
using BudgetTracker.Services;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _service;

    public ExpensesController(IExpenseService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] ExpenseCategory? category,
        [FromQuery] int? month,
        [FromQuery] int? year)
    {
        var expenses = await _service.GetAllAsync(category, month, year);
        return Ok(expenses);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var expense = await _service.GetByIdAsync(id);
        return expense is null ? NotFound() : Ok(expense);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExpenseRequest request)
    {
        var expense = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = expense.Id }, expense);
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateExpenseRequest request)
    {
        var expense = await _service.UpdateAsync(id, request);
        return expense is null ? NotFound() : Ok(expense);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        [FromQuery] int? month,
        [FromQuery] int? year)
    {
        var summary = await _service.GetSummaryAsync(month, year);
        return Ok(summary);
    }

    [HttpGet("report/{year:int}/{month:int}")]
    public async Task<IActionResult> GetMonthlyReport(int year, int month)
    {
        if (month < 1 || month > 12) return BadRequest("Month must be between 1 and 12");
        var report = await _service.GetMonthlyReportAsync(month, year);
        return Ok(report);
    }
}
