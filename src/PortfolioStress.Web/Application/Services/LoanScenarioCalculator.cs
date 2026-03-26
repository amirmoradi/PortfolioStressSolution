using PortfolioStress.Web.Application.Interfaces;
using PortfolioStress.Web.Application.Models;
using PortfolioStress.Web.Application.Models.Policy;
using PortfolioStress.Web.Domain.SourceData;

namespace PortfolioStress.Web.Application.Services;

/// <summary>
/// Performs calculation for a single loan.
/// 
/// Idea: I split this out from ScenarioCalculationService because the math started to get messy,
/// and I wanted something that can be unit tested in isolation.
/// Also the exercise wording was a bit ambiguous, so calculation behaviour is controlled
/// by CalculationPolicyOptions instead of hardcoding the formula.
/// </summary>
public class LoanScenarioCalculator : ILoanScenarioCalculator
{
    public LoanCalculationResult Calculate(LoanRecord loan, decimal probabilityOfDefault, decimal countryPercentageChange, CalculationPolicyOptions calculationPolicyOptions)
    {
        // PD should already be in range but I keep the check here just in case
        if (probabilityOfDefault < 0m || probabilityOfDefault > 1m)
        {
            throw new ArgumentOutOfRangeException(nameof(probabilityOfDefault), "Probability of default must be between 0 and 1.");
        }

        // input comes as percentage (-5.12 etc) so convert to decimal
        var normalizedChange = countryPercentageChange / 100m;

        // Depending on policy we either apply the change directly or deal with it as a stress to current collateral value.
        // I added this because the wording in the exercise can be interpreted both ways.
        var scenarioCollateralValue = calculationPolicyOptions.ScenarioCollateralFormula switch
        {
            ScenarioCollateralFormula.ChangeOnly => loan.CollateralValue * normalizedChange,

            ScenarioCollateralFormula.StressCurrentValue => loan.CollateralValue * (1m + normalizedChange),

            _ => throw new InvalidOperationException($"Unsupported scenario collateral formula '{calculationPolicyOptions.ScenarioCollateralFormula}'.")
        };

        // Recovery rate denominator also depends on policy.
        // made this a bit more complex than just using outstanding amount because different institutions use different formulas for recovery rate calculation,
        // so I made it configurable.
        var recoveryRateBaseAmount = calculationPolicyOptions.RecoveryRateBase switch
        {
            RecoveryRateBase.OriginalLoanAmount => loan.OriginalLoanAmount,

            RecoveryRateBase.OutstandingAmount => loan.OutstandingAmount,

            _ => throw new InvalidOperationException($"Unsupported recovery rate base '{calculationPolicyOptions.RecoveryRateBase}'.")
        };

        if (recoveryRateBaseAmount <= 0m)
        {
            throw new InvalidOperationException($"Loan {loan.LoanId} has a non-positive recovery rate denominator.");
        }

        // RR = scenario collateral / base amount
        var recoveryRate = scenarioCollateralValue / recoveryRateBaseAmount;

        // LGD = 1 - RR
        var lossGivenDefault = 1m - recoveryRate;

        // Some policies clamp negative LGD to zero, others don't.
        // I left this configurable so both behaviours can be tested.
        if (calculationPolicyOptions.ClampNegativeLossGivenDefaultToZero && lossGivenDefault < 0m)
        {
            lossGivenDefault = 0m;
        }

        // EL = PD * LGD * Outstanding
        var expectedLoss = probabilityOfDefault * lossGivenDefault * loan.OutstandingAmount;

        return new LoanCalculationResult(ScenarioCollateralValue: scenarioCollateralValue, RecoveryRate: recoveryRate, LossGivenDefault: lossGivenDefault, ExpectedLoss: expectedLoss);
    }
}