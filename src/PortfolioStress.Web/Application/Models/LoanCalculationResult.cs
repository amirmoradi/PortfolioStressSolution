namespace PortfolioStress.Web.Application.Models;

public record LoanCalculationResult(decimal ScenarioCollateralValue, decimal RecoveryRate, decimal LossGivenDefault, decimal ExpectedLoss);
