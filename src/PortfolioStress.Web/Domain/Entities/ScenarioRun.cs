namespace PortfolioStress.Web.Domain.Entities;

public class ScenarioRun
{
    public long Id { get; set; }
    public DateTime StartedUtc { get; set; }
    public DateTime CompletedUtc { get; set; }
    public long DurationMilliseconds { get; set; }
    public string CalculationProfile { get; set; } = string.Empty;
    public string ScenarioCollateralFormula { get; set; } = string.Empty;
    public string RecoveryRateBase { get; set; } = string.Empty;
    public bool ClampNegativeLossGivenDefaultToZero { get; set; }
    public string SourcePortfoliosPath { get; set; } = string.Empty;
    public string SourceLoansPath { get; set; } = string.Empty;
    public string SourceRatingsPath { get; set; } = string.Empty;
    public int PortfolioCount { get; set; }
    public int LoanCount { get; set; }
    public int ResultCount { get; set; }
    public decimal TotalOutstandingAmount { get; set; }
    public decimal TotalCollateralValue { get; set; }
    public decimal TotalScenarioCollateralValue { get; set; }
    public decimal TotalExpectedLoss { get; set; }
    public ICollection<ScenarioCountryInput> CountryInputs { get; set; } = new List<ScenarioCountryInput>();
    public ICollection<ScenarioPortfolioResult> PortfolioResults { get; set; } = new List<ScenarioPortfolioResult>();
}
