using Microsoft.EntityFrameworkCore;
using PortfolioStress.Web.Application.Interfaces;
using PortfolioStress.Web.Application.Services;
using PortfolioStress.Web.Infrastructure.Csv;
using PortfolioStress.Web.Infrastructure.Persistence;
using PortfolioStress.Web.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.Configure<ScenarioSourceFilesOptions>(builder.Configuration.GetSection(ScenarioSourceFilesOptions.SectionName));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IScenarioSourceDataReader, ScenarioSourceDataReader>();
builder.Services.AddSingleton<ILoanScenarioCalculator, LoanScenarioCalculator>();
builder.Services.AddScoped<IScenarioCalculationService, ScenarioCalculationService>();
builder.Services.AddScoped<IScenarioExecutionService, ScenarioExecutionService>();

var app = builder.Build();

Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "App_Data"));

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Runs/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Runs}/{action=Index}/{id?}");

app.Run();
