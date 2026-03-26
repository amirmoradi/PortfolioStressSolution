namespace PortfolioStress.Web.ViewModels;

public class RunListItemViewModel
{
    public long RunId { get; init; }
    public DateTime StartedUtc { get; init; }
    public long DurationMilliseconds { get; init; }
    public string CalculationProfile { get; init; } = string.Empty;
    public string CountryInputsSummary { get; init; } = string.Empty;
    public int LoanCount { get; init; }
    public decimal TotalOutstandingAmount { get; init; }
    public decimal TotalExpectedLoss { get; init; }
}
