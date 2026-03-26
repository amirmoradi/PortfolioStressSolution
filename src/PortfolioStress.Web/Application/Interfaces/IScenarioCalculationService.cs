using PortfolioStress.Web.Application.Models;

namespace PortfolioStress.Web.Application.Interfaces;

/// <summary>
/// Runs a full scenario calculation.
/// </summary>
public interface IScenarioCalculationService
{
    ScenarioExecutionResult Calculate(CreateScenarioRunCommand command);
}
