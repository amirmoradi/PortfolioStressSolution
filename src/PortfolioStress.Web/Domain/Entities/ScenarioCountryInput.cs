namespace PortfolioStress.Web.Domain.Entities;

public class ScenarioCountryInput
{
    public long Id { get; set; }
    public long ScenarioRunId { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public decimal PercentageChange { get; set; }
    public ScenarioRun ScenarioRun { get; set; } = null!;
}
