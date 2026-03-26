namespace PortfolioStress.Web.Options;

public class ScenarioSourceFilesOptions
{
    public const string SectionName = "ScenarioSourceFiles";

    public string PortfoliosPath { get; set; } = string.Empty;
    public string LoansPath { get; set; } = string.Empty;
    public string RatingsPath { get; set; } = string.Empty;
}
