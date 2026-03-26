namespace PortfolioStress.Web.ViewModels;

public class RunDetailsViewModel
{
    public long RunId { get; init; }
    public DateTime StartedUtc { get; init; }
    public DateTime CompletedUtc { get; init; }
    public long DurationMilliseconds { get; init; }
    public string CalculationProfile { get; init; } = string.Empty;
    public string ScenarioCollateralFormula { get; init; } = string.Empty;
    public string RecoveryRateBase { get; init; } = string.Empty;
    public bool ClampNegativeLossGivenDefaultToZero { get; init; }
    public string SourcePortfoliosPath { get; init; } = string.Empty;
    public string SourceLoansPath { get; init; } = string.Empty;
    public string SourceRatingsPath { get; init; } = string.Empty;
    public int PortfolioCount { get; init; }
    public int LoanCount { get; init; }
    public int ResultCount { get; init; }
    public decimal TotalOutstandingAmount { get; init; }
    public decimal TotalCollateralValue { get; init; }
    public decimal TotalScenarioCollateralValue { get; init; }
    public decimal TotalExpectedLoss { get; init; }
    public IReadOnlyList<RunCountryInputViewModel> CountryInputs { get; init; } = Array.Empty<RunCountryInputViewModel>();
    public IReadOnlyList<RunPortfolioResultViewModel> PortfolioResults { get; init; } = Array.Empty<RunPortfolioResultViewModel>();
}

public class RunCountryInputViewModel
{
    public string CountryCode { get; init; } = string.Empty;
    public decimal PercentageChange { get; init; }
}

public class RunPortfolioResultViewModel
{
    public int PortfolioId { get; init; }
    public string PortfolioName { get; init; } = string.Empty;
    public string CountryCode { get; init; } = string.Empty;
    public string CurrencyCode { get; init; } = string.Empty;
    public int LoanCount { get; init; }
    public decimal TotalOutstandingAmount { get; init; }
    public decimal TotalCollateralValue { get; init; }
    public decimal TotalScenarioCollateralValue { get; init; }
    public decimal TotalExpectedLoss { get; init; }
}
