using PortfolioStress.Web.Domain.SourceData;

namespace PortfolioStress.Web.Application.Interfaces;

/// <summary>
/// lodas source data needed for calculations.
/// </summary>
public interface IScenarioSourceDataReader
{
    IReadOnlyDictionary<int, PortfolioRecord> ReadPortfolios(string path);
    IReadOnlyDictionary<string, RatingRecord> ReadRatings(string path);
    IEnumerable<LoanRecord> ReadLoans(string path);
}
