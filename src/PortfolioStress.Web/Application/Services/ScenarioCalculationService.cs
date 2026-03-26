using System.Diagnostics;
using Microsoft.Extensions.Options;
using PortfolioStress.Web.Application.Interfaces;
using PortfolioStress.Web.Application.Models;
using PortfolioStress.Web.Application.Models.Policy;
using PortfolioStress.Web.Options;

namespace PortfolioStress.Web.Application.Services;

/// <summary>
/// Responsible for running the full scenario calculation.
/// 
/// Idea: I decided to keep this as a separate service instead of putting the logic in the controller,
/// because once I started wiring CSV reading + aggregation + calculation it got messy very fast.
/// This class basically coordinates reading the source data and producing the final aggregated result.
/// </summary>
public class ScenarioCalculationService : IScenarioCalculationService
{
    private readonly IScenarioSourceDataReader _scenarioSourceDataReader;
    private readonly ILoanScenarioCalculator _loanScenarioCalculator;
    private readonly ScenarioSourceFilesOptions _sourceFilesOptions;
    private readonly IWebHostEnvironment _environment;

    public ScenarioCalculationService(IScenarioSourceDataReader scenarioSourceDataReader, ILoanScenarioCalculator loanScenarioCalculator, IOptions<ScenarioSourceFilesOptions> sourceFilesOptions, IWebHostEnvironment environment)
    {
        _scenarioSourceDataReader = scenarioSourceDataReader;
        _loanScenarioCalculator = loanScenarioCalculator;
        _sourceFilesOptions = sourceFilesOptions.Value;
        _environment = environment;
    }

    public ScenarioExecutionResult Calculate(CreateScenarioRunCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        // normalize country inputs early so rest of code does not need to worry about casing / missing values
        var countryPercentageChanges = ValidateAndNormalizeCountryInputs(command.CountryPercentageChanges);

        // I added calculation profiles later when I realised the wording of the exercise
        // could be interpreted in more than one way, so this allows switching behaviour without
        // rewriting the calculation code.
        var calculationPolicy = CalculationProfiles.Resolve(command.CalculationProfile);

        // resolve file paths from config
        var portfoliosPath = ResolveExistingPath(_sourceFilesOptions.PortfoliosPath);
        var loansPath = ResolveExistingPath(_sourceFilesOptions.LoansPath);
        var ratingsPath = ResolveExistingPath(_sourceFilesOptions.RatingsPath);

        var startedUtc = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        // read source data
        var portfolios = _scenarioSourceDataReader.ReadPortfolios(portfoliosPath);
        var ratings = _scenarioSourceDataReader.ReadRatings(ratingsPath);

        // create aggregation containers per portfolio
        var portfolioAggregations = portfolios.Values.ToDictionary(
            portfolio => portfolio.PortfolioId,
            portfolio => new PortfolioAggregationResult
            {
                PortfolioId = portfolio.PortfolioId,
                PortfolioName = portfolio.PortfolioName,
                CountryCode = portfolio.CountryCode,
                CurrencyCode = portfolio.CurrencyCode
            });

        var processedLoanCount = 0;

        // iterate through loans one by one, calculate and add to aggregation
        foreach (var loan in _scenarioSourceDataReader.ReadLoans(loansPath))
        {
            if (!portfolios.TryGetValue(loan.PortfolioId, out var portfolio))
            {
                throw new InvalidOperationException(
                    $"Loan {loan.LoanId} references unknown portfolio id {loan.PortfolioId}.");
            }

            if (!ratings.TryGetValue(loan.CreditRating, out var rating))
            {
                throw new InvalidOperationException(
                    $"Loan {loan.LoanId} references unknown credit rating '{loan.CreditRating}'.");
            }

            if (!countryPercentageChanges.TryGetValue(
                    portfolio.CountryCode,
                    out var countryPercentageChange))
            {
                throw new InvalidOperationException(
                    $"Missing scenario input for country '{portfolio.CountryCode}'.");
            }

            // delegate the actual math to calculator service
            var loanCalculation = _loanScenarioCalculator.Calculate(
                loan,
                rating.ProbabilityOfDefault,
                countryPercentageChange,
                calculationPolicy.Options);

            var aggregation = portfolioAggregations[portfolio.PortfolioId];

            // accumulate totals per portfolio
            aggregation.LoanCount += 1;
            aggregation.TotalOutstandingAmount += loan.OutstandingAmount;
            aggregation.TotalCollateralValue += loan.CollateralValue;
            aggregation.TotalScenarioCollateralValue += loanCalculation.ScenarioCollateralValue;
            aggregation.TotalExpectedLoss += loanCalculation.ExpectedLoss;

            processedLoanCount += 1;
        }

        stopwatch.Stop();

        // I keep both start and end time mainly for debugging / logging later
        var completedUtc = startedUtc.Add(stopwatch.Elapsed);

        var orderedResults = portfolioAggregations.Values
            .OrderBy(result => result.PortfolioName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new ScenarioExecutionResult
        {
            StartedUtc = startedUtc,
            CompletedUtc = completedUtc,
            DurationMilliseconds = stopwatch.ElapsedMilliseconds,

            SourcePortfoliosPath = portfoliosPath,
            SourceLoansPath = loansPath,
            SourceRatingsPath = ratingsPath,

            CalculationPolicy = calculationPolicy,
            CountryPercentageChanges = countryPercentageChanges,

            PortfolioCount = portfolios.Count,
            LoanCount = processedLoanCount,
            ResultCount = orderedResults.Count,

            TotalOutstandingAmount = orderedResults.Sum(x => x.TotalOutstandingAmount),
            TotalCollateralValue = orderedResults.Sum(x => x.TotalCollateralValue),
            TotalScenarioCollateralValue = orderedResults.Sum(x => x.TotalScenarioCollateralValue),
            TotalExpectedLoss = orderedResults.Sum(x => x.TotalExpectedLoss),

            PortfolioResults = orderedResults
        };
    }

    /// <summary>
    /// Validates country inputs and normalizes keys to uppercase.
    /// Also checks that all required countries are present.
    /// </summary>
    private IReadOnlyDictionary<string, decimal> ValidateAndNormalizeCountryInputs(
        IReadOnlyDictionary<string, decimal> countryPercentageChanges)
    {
        if (countryPercentageChanges is null)
        {
            throw new ArgumentNullException(nameof(countryPercentageChanges));
        }

        var normalized = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        foreach (var input in countryPercentageChanges)
        {
            if (string.IsNullOrWhiteSpace(input.Key))
            {
                throw new InvalidOperationException("Country code cannot be blank.");
            }

            normalized[input.Key.Trim().ToUpperInvariant()] = input.Value;
        }

        // make sure user entered all countries from the exercise list
        var missingCountries = ScenarioCountries.All
            .Where(country => !normalized.ContainsKey(country))
            .ToList();

        if (missingCountries.Count > 0)
        {
            throw new InvalidOperationException($"Missing scenario inputs for: {string.Join(", ", missingCountries)}.");
        }

        return normalized;
    }

    /// <summary>
    /// Resolves relative path from config and ensures file exists.
    /// </summary>
    private string ResolveExistingPath(string configuredPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            throw new InvalidOperationException("One or more scenario source file paths are missing from configuration.");
        }

        var absolutePath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.GetFullPath(
                Path.Combine(_environment.ContentRootPath, configuredPath));

        if (!File.Exists(absolutePath))
        {
            throw new FileNotFoundException($"Expected source file was not found: {absolutePath}", absolutePath);
        }

        return absolutePath;
    }
}