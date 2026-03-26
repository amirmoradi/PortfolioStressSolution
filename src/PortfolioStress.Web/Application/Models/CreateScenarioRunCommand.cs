using PortfolioStress.Web.Application.Models.Policy;

namespace PortfolioStress.Web.Application.Models;

public class CreateScenarioRunCommand
{
    public CalculationProfile CalculationProfile { get; init; }

    public IReadOnlyDictionary<string, decimal> CountryPercentageChanges { get; init; } = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
}
