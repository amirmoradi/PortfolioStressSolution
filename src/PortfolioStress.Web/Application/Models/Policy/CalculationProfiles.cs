namespace PortfolioStress.Web.Application.Models.Policy
{
    public static class CalculationProfiles
    {
        public static IReadOnlyList<CalculationPolicyDefinition> All { get; } =
            new List<CalculationPolicyDefinition>
            {
            new(
                CalculationProfile.ExerciseDetails,
                "Exercise wording",
                "Keeps closest to the written exercise: scenario collateral equals collateral multiplied by the entered percentage change, recovery rate uses OriginalLoanAmount, and LGD is not clamped.",
                new CalculationPolicyOptions(
                    ScenarioCollateralFormula.ChangeOnly,
                    RecoveryRateBase.OriginalLoanAmount,
                    ClampNegativeLossGivenDefaultToZero: false)),
            new(
                CalculationProfile.MarketStress,
                "Market stress",
                "Applies the percentage change as a house price stress to current collateral, uses OutstandingAmount as the recovery base, and clamps negative LGD to zero.",
                new CalculationPolicyOptions(
                    ScenarioCollateralFormula.StressCurrentValue,
                    RecoveryRateBase.OutstandingAmount,
                    ClampNegativeLossGivenDefaultToZero: true))
            };

        public static CalculationPolicyDefinition Resolve(CalculationProfile profile) => All.Single(x => x.Profile == profile);
    }
}
