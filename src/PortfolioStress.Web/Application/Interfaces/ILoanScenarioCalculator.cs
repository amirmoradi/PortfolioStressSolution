using PortfolioStress.Web.Application.Models;
using PortfolioStress.Web.Application.Models.Policy;
using PortfolioStress.Web.Domain.SourceData;

namespace PortfolioStress.Web.Application.Interfaces;

/// <summary>
/// Calculates scenario results for a single loan.
/// </summary>
public interface ILoanScenarioCalculator
{
    LoanCalculationResult Calculate(LoanRecord loan, decimal probabilityOfDefault, decimal countryPercentageChange, CalculationPolicyOptions calculationPolicyOptions);
}
