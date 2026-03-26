using PortfolioStress.Web.Application.Interfaces;
using PortfolioStress.Web.Application.Models;
using PortfolioStress.Web.Application.Models.Policy;
using PortfolioStress.Web.Domain.Entities;
using PortfolioStress.Web.Infrastructure.Persistence;

namespace PortfolioStress.Web.Application.Services;

/// <summary>
/// Defines the contract for running a full scenario calculation.
/// 
/// Responsibilities:
/// - Accepting country-level percentages.
/// - Coordinating reading of source data
/// - Applying the selected calculation policy for each loan.
/// - Results at the portfolio level.
/// - Persisting the run metadata and results.
/// 
/// </summary>
public class ScenarioExecutionService : IScenarioExecutionService
{
    private readonly IScenarioCalculationService _scenarioCalculationService;
    private readonly AppDbContext _dbContext;

    public ScenarioExecutionService(IScenarioCalculationService scenarioCalculationService, AppDbContext dbContext)
    {
        _scenarioCalculationService = scenarioCalculationService;
        _dbContext = dbContext;
    }

    public async Task<long> ExecuteAsync(CreateScenarioRunCommand command, CancellationToken cancellationToken = default)
    {
        var calculation = _scenarioCalculationService.Calculate(command);

        var run = new ScenarioRun
        {
            StartedUtc = calculation.StartedUtc,
            CompletedUtc = calculation.CompletedUtc,
            DurationMilliseconds = calculation.DurationMilliseconds,
            CalculationProfile = calculation.CalculationPolicy.Name,
            ScenarioCollateralFormula = calculation.CalculationPolicy.Options.ScenarioCollateralFormula.ToDisplayText(),
            RecoveryRateBase = calculation.CalculationPolicy.Options.RecoveryRateBase.ToDisplayText(),
            ClampNegativeLossGivenDefaultToZero = calculation.CalculationPolicy.Options.ClampNegativeLossGivenDefaultToZero,
            SourcePortfoliosPath = calculation.SourcePortfoliosPath,
            SourceLoansPath = calculation.SourceLoansPath,
            SourceRatingsPath = calculation.SourceRatingsPath,
            PortfolioCount = calculation.PortfolioCount,
            LoanCount = calculation.LoanCount,
            ResultCount = calculation.ResultCount,
            TotalOutstandingAmount = calculation.TotalOutstandingAmount,
            TotalCollateralValue = calculation.TotalCollateralValue,
            TotalScenarioCollateralValue = calculation.TotalScenarioCollateralValue,
            TotalExpectedLoss = calculation.TotalExpectedLoss,
            CountryInputs = calculation.CountryPercentageChanges
                .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
                .Select(x => new ScenarioCountryInput
                {
                    CountryCode = x.Key,
                    PercentageChange = x.Value
                })
                .ToList(),
            PortfolioResults = calculation.PortfolioResults
                .OrderBy(x => x.PortfolioName, StringComparer.OrdinalIgnoreCase)
                .Select(x => new ScenarioPortfolioResult
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

        _dbContext.ScenarioRuns.Add(run);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return run.Id;
    }
}
