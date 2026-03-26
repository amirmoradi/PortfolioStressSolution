using PortfolioStress.Web.Application.Models;
using PortfolioStress.Web.Application.Models.Policy;
using PortfolioStress.Web.Application.Services;
using PortfolioStress.Web.Domain.SourceData;
using Xunit;

namespace PortfolioStress.Tests;

public class LoanScenarioCalculatorTests
{
    private readonly LoanScenarioCalculator _calculator = new();

    [Fact]
    public void ExerciseWordingPolicy_UsesLiteralScenarioFormula()
    {
        var loan = new LoanRecord(1, 1, 100m, 80m, 120m, "A");
        var policy = CalculationProfiles.Resolve(CalculationProfile.ExerciseDetails);

        var result = _calculator.Calculate(loan, 0.25m, -10m, policy.Options);

        Assert.Equal(-12m, result.ScenarioCollateralValue);
        Assert.Equal(-0.12m, result.RecoveryRate);
        Assert.Equal(1.12m, result.LossGivenDefault);
        Assert.Equal(22.40m, result.ExpectedLoss);
    }

    [Fact]
    public void MarketStressPolicy_AppliesstressAndClampsNegativeLgdToZero()
    {
        var loan = new LoanRecord(2, 1, 100m, 80m, 120m, "A");
        var policy = CalculationProfiles.Resolve(CalculationProfile.MarketStress);

        var result = _calculator.Calculate(loan, 0.25m, -10m, policy.Options);

        Assert.Equal(108m, result.ScenarioCollateralValue);
        Assert.Equal(1.35m, result.RecoveryRate);
        Assert.Equal(0m, result.LossGivenDefault);
        Assert.Equal(0m, result.ExpectedLoss);
    }
}
