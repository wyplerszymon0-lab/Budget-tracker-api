using BudgetTracker.Data;
using BudgetTracker.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseInMemoryDatabase("BudgetTrackerDb"));

builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IBudgetService,  BudgetService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();

public partial class Program {}
