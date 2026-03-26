namespace PortfolioStress.Web.Application.Models.Policy
{
    public static class CalculationPolicyDisplayExtensions
    {
        public static string ToDisplayText(this ScenarioCollateralFormula formula) =>
            formula switch
            {
                ScenarioCollateralFormula.ChangeOnly => "CollateralValue * PercentageChange%",
                ScenarioCollateralFormula.StressCurrentValue => "CollateralValue * (1 + PercentageChange% / 100)",
                _ => formula.ToString()
            };

        public static string ToDisplayText(this RecoveryRateBase recoveryRateBase) =>
            recoveryRateBase switch
            {
                RecoveryRateBase.OriginalLoanAmount => "OriginalLoanAmount",
                RecoveryRateBase.OutstandingAmount => "OutstandingAmount",
                _ => recoveryRateBase.ToString()
            };
    }
}
