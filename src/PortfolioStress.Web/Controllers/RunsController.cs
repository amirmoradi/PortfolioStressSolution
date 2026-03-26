using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PortfolioStress.Web.Application.Interfaces;
using PortfolioStress.Web.Application.Models;
using PortfolioStress.Web.Application.Models.Policy;
using PortfolioStress.Web.Infrastructure.Persistence;
using PortfolioStress.Web.ViewModels;

namespace PortfolioStress.Web.Controllers;

public class RunsController : Controller
{
    private readonly IScenarioExecutionService _scenarioExecutionService;
    private readonly AppDbContext _dbContext;

    public RunsController(IScenarioExecutionService scenarioExecutionService, AppDbContext dbContext)
    {
        _scenarioExecutionService = scenarioExecutionService;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var runs = await _dbContext.ScenarioRuns
            .AsNoTracking()
            .Include(x => x.CountryInputs)
            .OrderByDescending(x => x.StartedUtc)
            .ToListAsync(cancellationToken);

        var model = runs
            .Select(run => new RunListItemViewModel
            {
                RunId = run.Id,
                StartedUtc = run.StartedUtc,
                DurationMilliseconds = run.DurationMilliseconds,
                CalculationProfile = run.CalculationProfile,
                CountryInputsSummary = string.Join(
                    ", ",
                    run.CountryInputs
                        .OrderBy(x => x.CountryCode, StringComparer.OrdinalIgnoreCase)
                        .Select(x => $"{x.CountryCode}={x.PercentageChange:0.##}%")),
                LoanCount = run.LoanCount,
                TotalOutstandingAmount = run.TotalOutstandingAmount,
                TotalExpectedLoss = run.TotalExpectedLoss
            })
            .ToList();

        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var model = BuildCreateRunViewModel();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRunViewModel model, CancellationToken cancellationToken)
    {
        PopulateReferenceData(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var command = new CreateScenarioRunCommand
            {
                CalculationProfile = model.CalculationProfile,
                CountryPercentageChanges = model.Countrystresss.ToDictionary(
                    x => x.CountryCode,
                    x => x.PercentageChange!.Value,
                    StringComparer.OrdinalIgnoreCase)
            };

            var runId = await _scenarioExecutionService.ExecuteAsync(command, cancellationToken);
            return RedirectToAction(nameof(Details), new { id = runId });
        }
        catch (Exception exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(long id, CancellationToken cancellationToken)
    {
        var run = await _dbContext.ScenarioRuns
            .AsNoTracking()
            .Include(x => x.CountryInputs)
            .Include(x => x.PortfolioResults)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (run is null)
        {
            return NotFound();
        }

        var model = new RunDetailsViewModel
        {
            RunId = run.Id,
            StartedUtc = run.StartedUtc,
            CompletedUtc = run.CompletedUtc,
            DurationMilliseconds = run.DurationMilliseconds,
            CalculationProfile = run.CalculationProfile,
            ScenarioCollateralFormula = run.ScenarioCollateralFormula,
            RecoveryRateBase = run.RecoveryRateBase,
            ClampNegativeLossGivenDefaultToZero = run.ClampNegativeLossGivenDefaultToZero,
            SourcePortfoliosPath = run.SourcePortfoliosPath,
            SourceLoansPath = run.SourceLoansPath,
            SourceRatingsPath = run.SourceRatingsPath,
            PortfolioCount = run.PortfolioCount,
            LoanCount = run.LoanCount,
            ResultCount = run.ResultCount,
            TotalOutstandingAmount = run.TotalOutstandingAmount,
            TotalCollateralValue = run.TotalCollateralValue,
            TotalScenarioCollateralValue = run.TotalScenarioCollateralValue,
            TotalExpectedLoss = run.TotalExpectedLoss,
            CountryInputs = run.CountryInputs
                .OrderBy(x => x.CountryCode, StringComparer.OrdinalIgnoreCase)
                .Select(x => new RunCountryInputViewModel
                {
                    CountryCode = x.CountryCode,
                    PercentageChange = x.PercentageChange
                })
                .ToList(),
            PortfolioResults = run.PortfolioResults
                .OrderBy(x => x.PortfolioName, StringComparer.OrdinalIgnoreCase)
                .Select(x => new RunPortfolioResultViewModel
                {
                    PortfolioId = x.PortfolioId,
                    PortfolioName = x.PortfolioName,
                    CountryCode = x.CountryCode,
                    CurrencyCode = x.CurrencyCode,
                    LoanCount = x.LoanCount,
                    TotalOutstandingAmount = x.TotalOutstandingAmount,
                    TotalCollateralValue = x.TotalCollateralValue,
                    TotalScenarioCollateralValue = x.TotalScenarioCollateralValue,
                    TotalExpectedLoss = x.TotalExpectedLoss
                })
                .ToList()
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult Error() => View();

    private static CreateRunViewModel BuildCreateRunViewModel()
    {
        var defaults = new Dictionary<string, decimal>
        {
            ["GB"] = -5.12m,
            ["US"] = -4.34m,
            ["FR"] = -3.87m,
            ["DE"] = -1.23m,
            ["SG"] = -5.50m,
            ["GR"] = -5.68m
        };

        var model = new CreateRunViewModel
        {
            Countrystresss = ScenarioCountries.All
                .Select(country => new CountrystressInputViewModel
                {
                    CountryCode = country,
                    PercentageChange = defaults[country]
                })
                .ToList()
        };

        PopulateReferenceData(model);
        return model;
    }

    private static void PopulateReferenceData(CreateRunViewModel model)
    {
        model.AvailableCalculationProfiles = CalculationProfiles.All
            .Select(policy => new SelectListItem(policy.Name, ((int)policy.Profile).ToString(), policy.Profile == model.CalculationProfile))
            .ToList();

        model.CalculationProfileDescriptions = CalculationProfiles.All
            .Select(policy => new CalculationProfileOptionViewModel
            {
                Name = policy.Name,
                Description = policy.Description
            })
            .ToList();

        if (model.Countrystresss.Count == 0)
        {
            model.Countrystresss = BuildCreateRunViewModel().Countrystresss;
        }
    }
}
