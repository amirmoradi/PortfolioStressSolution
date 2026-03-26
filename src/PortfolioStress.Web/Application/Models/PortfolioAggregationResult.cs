namespace PortfolioStress.Web.Application.Models;

public class PortfolioAggregationResult
{
    public int PortfolioId { get; init; }
    public string PortfolioName { get; init; } = string.Empty;
    public string CountryCode { get; init; } = string.Empty;
    public string CurrencyCode { get; init; } = string.Empty;
    public int LoanCount { get; set; }
    public decimal TotalOutstandingAmount { get; set; }
    public decimal TotalCollateralValue { get; set; }
    public decimal TotalScenarioCollateralValue { get; set; }
    public decimal TotalExpectedLoss { get; set; }
}
