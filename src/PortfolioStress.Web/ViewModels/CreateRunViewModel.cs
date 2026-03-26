using Microsoft.AspNetCore.Mvc.Rendering;
using PortfolioStress.Web.Application.Models;
using PortfolioStress.Web.Application.Models.Policy;

namespace PortfolioStress.Web.ViewModels;

public class CreateRunViewModel
{
    public CalculationProfile CalculationProfile { get; set; } = CalculationProfile.ExerciseDetails;

    public List<CountrystressInputViewModel> Countrystresss { get; set; } = new();

    public IReadOnlyList<SelectListItem> AvailableCalculationProfiles { get; set; } = Array.Empty<SelectListItem>();
    public IReadOnlyList<CalculationProfileOptionViewModel> CalculationProfileDescriptions { get; set; } = Array.Empty<CalculationProfileOptionViewModel>();
}
