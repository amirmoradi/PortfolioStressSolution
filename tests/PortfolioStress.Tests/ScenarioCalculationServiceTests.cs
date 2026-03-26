using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using PortfolioStress.Web.Application.Interfaces;
using PortfolioStress.Web.Application.Models;
using PortfolioStress.Web.Application.Models.Policy;
using PortfolioStress.Web.Application.Services;
using PortfolioStress.Web.Domain.SourceData;
using PortfolioStress.Web.Options;
using Xunit;

namespace PortfolioStress.Tests;

public class ScenarioCalculationServiceTests
{
    [Fact]
    public void Calculate_GroupsLoansByPortfolioAndProducesExpectedTotals()
    {
        var tempDirectory = CreateTempDirectoryWithPlaceholderFiles();

        var sourceReader = new FakeScenarioSourceDataReader(
            portfolios: new Dictionary<int, PortfolioRecord>
            {
                [1] = new PortfolioRecord(1, "PORT01", "GB", "GBP"),
                [2] = new PortfolioRecord(2, "PORT02", "US", "USD")
            },
            ratings: new Dictionary<string, RatingRecord>(StringComparer.OrdinalIgnoreCase)
            {
                ["A"] = new RatingRecord("A", 0.25m),
                ["B"] = new RatingRecord("B", 0.75m)
            },
            loans: new[]
            {
                new LoanRecord(1, 1, 100m, 80m, 90m, "A"),
                new LoanRecord(2, 1, 200m, 150m, 100m, "B"),
                new LoanRecord(3, 2, 100m, 100m, 100m, "A")
            });

        var options = Options.Create(new ScenarioSourceFilesOptions
        {
            PortfoliosPath = "portfolios.csv",
            LoansPath = "loans.csv",
            RatingsPath = "ratings.csv"
        });

        var environment = new FakeWebHostEnvironment(tempDirectory);
        var calculator = new LoanScenarioCalculator();
        var service = new ScenarioCalculationService(sourceReader, calculator, options, environment);

        var result = service.Calculate(new CreateScenarioRunCommand
        {
            CalculationProfile = CalculationProfile.MarketStress,
            CountryPercentageChanges = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
            {
                ["GB"] = -10m,
                ["US"] = -5m,
                ["FR"] = 0m,
                ["DE"] = 0m,
                ["SG"] = 0m,
                ["GR"] = 0m
            }
        });

        Assert.Equal(2, result.PortfolioCount);
        Assert.Equal(3, result.LoanCount);
        Assert.Equal(2, result.ResultCount);
        Assert.Equal(330m, result.TotalOutstandingAmount);
        Assert.Equal(290m, result.TotalCollateralValue);
        Assert.Equal(266m, result.TotalScenarioCollateralValue);
        Assert.Equal(46.25m, result.TotalExpectedLoss);

        var portfolioOne = result.PortfolioResults.Single(x => x.PortfolioId == 1);
        Assert.Equal(2, portfolioOne.LoanCount);
        Assert.Equal(230m, portfolioOne.TotalOutstandingAmount);
        Assert.Equal(190m, portfolioOne.TotalCollateralValue);
        Assert.Equal(171m, portfolioOne.TotalScenarioCollateralValue);
        Assert.Equal(45m, portfolioOne.TotalExpectedLoss);
    }

    [Fact]
    public void Calculate_ThrowsWhenMandatoryCountryInputIsMissing()
    {
        var tempDirectory = CreateTempDirectoryWithPlaceholderFiles();

        var sourceReader = new FakeScenarioSourceDataReader(
            portfolios: new Dictionary<int, PortfolioRecord>(),
            ratings: new Dictionary<string, RatingRecord>(StringComparer.OrdinalIgnoreCase),
            loans: Array.Empty<LoanRecord>());

        var options = Options.Create(new ScenarioSourceFilesOptions
        {
            PortfoliosPath = "portfolios.csv",
            LoansPath = "loans.csv",
            RatingsPath = "ratings.csv"
        });

        var environment = new FakeWebHostEnvironment(tempDirectory);
        var calculator = new LoanScenarioCalculator();
        var service = new ScenarioCalculationService(sourceReader, calculator, options, environment);

        var exception = Assert.Throws<InvalidOperationException>(() => service.Calculate(new CreateScenarioRunCommand
        {
            CalculationProfile = CalculationProfile.ExerciseDetails,
            CountryPercentageChanges = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
            {
                ["GB"] = -1m,
                ["US"] = -1m
            }
        }));

        Assert.Contains("Missing scenario inputs", exception.Message);
    }

    private static string CreateTempDirectoryWithPlaceholderFiles()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "portfolios.csv"), "placeholder");
        File.WriteAllText(Path.Combine(directory, "loans.csv"), "placeholder");
        File.WriteAllText(Path.Combine(directory, "ratings.csv"), "placeholder");
        return directory;
    }

    private class FakeScenarioSourceDataReader : IScenarioSourceDataReader
    {
        private readonly IReadOnlyDictionary<int, PortfolioRecord> _portfolios;
        private readonly IReadOnlyDictionary<string, RatingRecord> _ratings;
        private readonly IEnumerable<LoanRecord> _loans;

        public FakeScenarioSourceDataReader(
            IReadOnlyDictionary<int, PortfolioRecord> portfolios,
            IReadOnlyDictionary<string, RatingRecord> ratings,
            IEnumerable<LoanRecord> loans)
        {
            _portfolios = portfolios;
            _ratings = ratings;
            _loans = loans;
        }

        public IReadOnlyDictionary<int, PortfolioRecord> ReadPortfolios(string path) => _portfolios;
        public IReadOnlyDictionary<string, RatingRecord> ReadRatings(string path) => _ratings;
        public IEnumerable<LoanRecord> ReadLoans(string path) => _loans;
    }

    private class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public FakeWebHostEnvironment(string contentRootPath)
        {
            ContentRootPath = contentRootPath;
            ContentRootFileProvider = new NullFileProvider();
            WebRootFileProvider = new NullFileProvider();
        }

        public string ApplicationName { get; set; } = "PortfolioStress.Tests";
        public IFileProvider WebRootFileProvider { get; set; }
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; }
    }
}
