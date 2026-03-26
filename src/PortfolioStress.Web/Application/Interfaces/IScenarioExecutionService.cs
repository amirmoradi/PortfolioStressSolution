using PortfolioStress.Web.Application.Models;

namespace PortfolioStress.Web.Application.Interfaces;

/// <summary>
/// Executes a scenario run and stores the results to the database.
/// </summary>
public interface IScenarioExecutionService
{
    Task<long> ExecuteAsync(CreateScenarioRunCommand command, CancellationToken cancellationToken = default);
}
