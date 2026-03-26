using System.ComponentModel.DataAnnotations;

namespace PortfolioStress.Web.ViewModels;

public class CountrystressInputViewModel
{
    [Required]
    public string CountryCode { get; set; } = "";

    [Required]
    public decimal? PercentageChange { get; set; }
}
