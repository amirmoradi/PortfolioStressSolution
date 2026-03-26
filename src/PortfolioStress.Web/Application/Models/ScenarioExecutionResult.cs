using PortfolioStress.Web.Application.Models.Policy;

namespace PortfolioStress.Web.Application.Models;

public class ScenarioExecutionResult
{
    public DateTime StartedUtc { get; init; }
    public DateTime CompletedUtc { get; init; }
    public long DurationMilliseconds { get; init; }
    public string SourcePortfoliosPath { get; init; } = string.Empty;
    public string SourceLoansPath { get; init; } = string.Empty;
    public string SourceRatingsPath { get; init; } = string.Empty;
    public CalculationPolicyDefinition CalculationPolicy { get; init; } = null!;
    public IReadOnlyDictionary<string, decimal> CountryPercentageChanges { get; init; } = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
    public int PortfolioCount { get; init; }
    public int LoanCount { get; init; }
    public int ResultCount { get; init; }
    public decimal TotalOutstandingAmount { get; init; }
    public decimal TotalCollateralValue { get; init; }
    public decimal TotalScenarioCollateralValue { get; init; }
    public decimal TotalExpectedLoss { get; init; }

    public IReadOnlyList<PortfolioAggregationResult> PortfolioResults { get; init; } = Array.Empty<PortfolioAggregationResult>();
}
