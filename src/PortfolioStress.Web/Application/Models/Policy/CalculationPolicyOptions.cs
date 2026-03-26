namespace PortfolioStress.Web.Application.Models.Policy
{
    public record CalculationPolicyOptions(ScenarioCollateralFormula ScenarioCollateralFormula, RecoveryRateBase RecoveryRateBase, bool ClampNegativeLossGivenDefaultToZero);

}
