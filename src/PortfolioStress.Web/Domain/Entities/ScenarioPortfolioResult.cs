namespace PortfolioStress.Web.Domain.Entities;

public class ScenarioPortfolioResult
{
    public long Id { get; set; }
    public long ScenarioRunId { get; set; }
    public int PortfolioId { get; set; }
    public string PortfolioName { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public int LoanCount { get; set; }
    public decimal TotalOutstandingAmount { get; set; }
    public decimal TotalCollateralValue { get; set; }
    public decimal TotalScenarioCollateralValue { get; set; }
    public decimal TotalExpectedLoss { get; set; }
    public ScenarioRun ScenarioRun { get; set; } = null!;
}
